﻿using PDDLSharp.Contextualisers;
using PDDLSharp.Models;
using PDDLSharp.Models.AST;
using PDDLSharp.Models.Domain;
using PDDLSharp.Models.Expressions;
using PDDLSharp.Models.Problem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace StacklebergCompiler
{
    public class ConditionalEffectCompiler
    {
        public PDDLDecl GenerateConditionalEffects(DomainDecl domain, ProblemDecl problem, ActionDecl metaAction)
        {
            var newDomain = domain.Copy();
            var newProblem = problem.Copy();

            // Find static predicates
            StaticPredicateDetector.GenerateStaticPredicates(domain);

            // Generate total goal
            GenerateIsGoal(newProblem, newDomain);

            // Problem
            GenerateNewInits(newProblem);
            GenerateNewGoal(newProblem, newDomain);
            InsertTurnPredicate(newProblem);

            // Domain
            GenerateLeaderFollowerActions(newDomain);
            TurnAllActionEffectsToAnd(newDomain);
            if (newDomain.Predicates != null)
            {
                var newPredicates = new List<PredicateExp>();
                newPredicates.AddRange(GenerateLeaderPredicates(newDomain));
                newPredicates.AddRange(GenerateGoalPredicates(newDomain));
                newDomain.Predicates.Predicates.AddRange(newPredicates);
            }
            UpdateLeaderActionsPredicates(newDomain);
            UpdateFollowerActionsEffects(newDomain);
            UpdateAndInsertMetaActionToFit(newDomain, metaAction);
            InsertTurnPredicate(newDomain);
            if (newProblem.Goal != null)
                AddFollowerReachGoalAction(newDomain, newProblem.Goal);

            return new PDDLDecl(newDomain, newProblem);
        }

        private void AddFollowerReachGoalAction(DomainDecl domain, GoalDecl goal)
        {
            domain.Actions.Add(new ActionDecl(
                null,
                ReservedNames.FollowerReachGoalAction,
                new ParameterExp(
                    null,
                    new List<NameExp>()),
                new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()),
                new NotExp(null, new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()))
                ));
        }

        private void TurnAllActionEffectsToAnd(DomainDecl domain)
        {
            foreach(var act in domain.Actions)
            {
                if (act.Effects is not AndExp)
                {
                    var newAnd = new AndExp(null, new List<IExp>()
                    {
                        act.Effects
                    });
                    act.Effects = newAnd;
                }
            }
        }

        private void InsertTurnPredicate(DomainDecl domain)
        {
            if (domain.Predicates != null)
                domain.Predicates.Predicates.Add(
                    new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()));

            foreach(var action in domain.Actions)
            {
                if (action.Name.Contains(ReservedNames.LeaderActionPrefix))
                {
                    if (action.Preconditions is AndExp leaderPreconditions)
                        leaderPreconditions.Children.Add(new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()));
                }
                else if (action.Name.Contains(ReservedNames.FollowerActionPrefix))
                {
                    if (action.Preconditions is AndExp leaderPreconditions)
                        leaderPreconditions.Children.Add(new NotExp(null, new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>())));
                }
            }
        }

        private void InsertTurnPredicate(ProblemDecl problem)
        {
            if (problem.Init != null)
                problem.Init.Predicates.Add(
                    new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()));
        }

        private void UpdateAndInsertMetaActionToFit(DomainDecl domain, ActionDecl metaAction)
        {
            if (metaAction.Preconditions is AndExp preAnd)
            {
                var preconditions = preAnd.FindTypes<PredicateExp>();
                foreach (var predicate in preconditions)
                {
                    if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                        predicate.Name = $"{ReservedNames.LeaderStatePrefix}{predicate.Name}";
                }
            }
            List<IExp> newChildren = new List<IExp>();
            var effects = metaAction.Effects.FindTypes<PredicateExp>();
            foreach (var effect in effects)
            {
                if (!StaticPredicateDetector.StaticPredicates.Contains(effect.Name))
                {
                    if (effect.Parent is NotExp not)
                    {
                        var newNormalEffect = effect.Copy();
                        newNormalEffect.Name = $"{ReservedNames.LeaderStatePrefix}{newNormalEffect.Name}";
                        newChildren.Add(new NotExp(null, newNormalEffect));
                    }
                    else
                    {
                        var newNormalEffect = effect.Copy();
                        newNormalEffect.Name = $"{ReservedNames.LeaderStatePrefix}{newNormalEffect.Name}";
                        newChildren.Add(newNormalEffect);
                    }

                    var newGoalEffect = effect.Copy();
                    newGoalEffect.Name = $"{ReservedNames.IsGoalPrefix}{newGoalEffect.Name}";
                    newChildren.Add(new NotExp(null, newGoalEffect));
                }
            }
            newChildren.Add(new NotExp(null, new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>())));
            metaAction.Effects = new AndExp(null, newChildren);

            metaAction.Name = $"{ReservedNames.LeaderActionPrefix}{ReservedNames.MetaActionPrefix}{metaAction.Name}";
            domain.Actions.Add(metaAction);
        }

        private void UpdateLeaderActionsPredicates(DomainDecl domain)
        {
            foreach (var action in domain.Actions.Where(x => x.Name.StartsWith(ReservedNames.LeaderActionPrefix)))
            {
                // Preconditions
                var preconditions = action.Preconditions.FindTypes<PredicateExp>();
                foreach (var predicate in preconditions)
                    if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                        predicate.Name = $"{ReservedNames.LeaderStatePrefix}{predicate.Name}";

                // Effects
                var leaderEffect = action.Effects.Copy();
                var predicates = leaderEffect.FindTypes<PredicateExp>();
                foreach (var predicate in predicates)
                    if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                        predicate.Name = $"{ReservedNames.LeaderStatePrefix}{predicate.Name}";

                var followerEffect = action.Effects.Copy();

                List<IExp> newChildren = new List<IExp>();
                if (leaderEffect is AndExp leaderAnd)
                    newChildren.AddRange(leaderAnd.Children);
                if (followerEffect is AndExp followerAnd)
                    newChildren.AddRange(followerAnd.Children);
                action.Effects = new AndExp(null, newChildren);
            }
        }

        private void UpdateFollowerActionsEffects(DomainDecl domain)
        {
            foreach (var action in domain.Actions.Where(x => x.Name.StartsWith(ReservedNames.FollowerActionPrefix)))
            {
                var newExpressions = new List<IExp>();
                var predicates = action.Effects.FindTypes<PredicateExp>();
                foreach (var pred in predicates)
                {
                    if (pred.Parent is NotExp)
                    {
                        var trueWhen = new WhenExp(action, null, null);
                        trueWhen.Condition = new NotExp(trueWhen, new PredicateExp(
                            trueWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments));
                        trueWhen.Effect = new PredicateExp(
                            trueWhen,
                            $"{ReservedNames.IsGoalPrefix}{pred.Name}",
                            pred.Arguments);
                        newExpressions.Add(trueWhen);

                        var falseWhen = new WhenExp(action, null, null);
                        falseWhen.Condition = new PredicateExp(
                            falseWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments);
                        falseWhen.Effect = new NotExp(null, new PredicateExp(
                            falseWhen,
                            $"{ReservedNames.IsGoalPrefix}{pred.Name}",
                            pred.Arguments));
                        newExpressions.Add(falseWhen);
                    }
                    else
                    {
                        var trueWhen = new WhenExp(action, null, null);
                        trueWhen.Condition = new PredicateExp(
                            trueWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments);
                        trueWhen.Effect = new PredicateExp(
                            trueWhen,
                            $"{ReservedNames.IsGoalPrefix}{pred.Name}",
                            pred.Arguments);
                        newExpressions.Add(trueWhen);

                        var falseWhen = new WhenExp(action, null, null);
                        falseWhen.Condition = new NotExp(null, new PredicateExp(
                            falseWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments));
                        falseWhen.Effect = new NotExp(null, new PredicateExp(
                            falseWhen,
                            $"{ReservedNames.IsGoalPrefix}{pred.Name}",
                            pred.Arguments));
                        newExpressions.Add(falseWhen);
                    }
                }

                if (action.Effects is AndExp and)
                    and.Children.AddRange(newExpressions);
            }
        }

        private List<PredicateExp> GenerateLeaderPredicates(DomainDecl domain)
        {
            var newLeaderPredicates = new List<PredicateExp>();
            if (domain.Predicates != null)
            {
                foreach (var predicate in domain.Predicates.Predicates)
                {
                    if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                        newLeaderPredicates.Add(new PredicateExp(
                            domain.Predicates,
                            $"{ReservedNames.LeaderStatePrefix}{predicate.Name}",
                            predicate.Arguments));
                }
            }
            return newLeaderPredicates;
        }

        private List<PredicateExp> GenerateGoalPredicates(DomainDecl domain)
        {
            var newGoalPredicates = new List<PredicateExp>();
            if (domain.Predicates != null)
            {
                foreach (var predicate in domain.Predicates.Predicates)
                {
                    if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                        newGoalPredicates.Add(new PredicateExp(
                            domain.Predicates,
                            $"{ReservedNames.IsGoalPrefix}{predicate.Name}",
                            predicate.Arguments));
                }
            }
            return newGoalPredicates;
        }

        private void GenerateLeaderFollowerActions(DomainDecl domain)
        {
            List<ActionDecl> newActions = new List<ActionDecl>();
            foreach(var action in domain.Actions)
            {
                var leaderAct = action.Copy();
                leaderAct.Name = $"{ReservedNames.LeaderActionPrefix}{leaderAct.Name}";
                var followerAct = action.Copy();
                followerAct.Name = $"{ReservedNames.FollowerActionPrefix}{followerAct.Name}";
                newActions.Add(leaderAct);
                newActions.Add(followerAct);
            }
            domain.Actions = newActions;
        }

        private void GenerateNewInits(ProblemDecl problem)
        {
            if (problem.Init != null)
            {
                var initPredicates = new List<PredicateExp>();
                foreach (var item in problem.Init.Predicates)
                    if (item is PredicateExp pred)
                        initPredicates.Add(pred);

                problem.Init.Predicates.AddRange(GenerateLeaderInits(initPredicates));
                problem.Init.Predicates.AddRange(IsGoal.Copy());
            }
        }

        private List<PredicateExp> GenerateLeaderInits(List<PredicateExp> inits)
        {
            var newInits = new List<PredicateExp>();
            foreach (var init in inits)
            {
                if (init is PredicateExp pred)
                    if (!StaticPredicateDetector.StaticPredicates.Contains(pred.Name))
                        newInits.Add(new PredicateExp(
                            null,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments));
            }
            return newInits;
        }

        private void GenerateNewGoal(ProblemDecl problem, DomainDecl domain)
        {
            if (problem.Goal != null && problem.Init != null && problem.Objects != null && domain.Predicates != null)
            {
                var goals = IsGoal.Copy();
                List<IExp> newGoals = new List<IExp>();
                foreach (var goal in goals)
                    newGoals.Add(goal);

                problem.Goal.GoalExp = new AndExp(null, newGoals);
            }
        }

        private List<PredicateExp> IsGoal = new List<PredicateExp>();
        private void GenerateIsGoal(ProblemDecl problem, DomainDecl domain)
        {
            if (problem.Goal != null && problem.Init != null && problem.Objects != null && domain.Predicates != null)
            {
                List<PredicateExp> newGoals = new List<PredicateExp>();
                var objDict = DictionaryObjectsOfType(problem.Objects.Objs);

                foreach (var predicate in domain.Predicates.Predicates)
                    newGoals.AddRange(GeneratePossibles(predicate, 0, objDict));
                newGoals.RemoveAll(x => StaticPredicateDetector.StaticPredicates.Contains((x as PredicateExp).Name));
                foreach (var predicate in newGoals)
                    predicate.Name = $"{ReservedNames.IsGoalPrefix}{predicate.Name}";
                IsGoal = newGoals;
            }
        }

        private List<PredicateExp> GeneratePossibles(PredicateExp parentPredicate, int argIndex, Dictionary<string, List<string>> objDict)
        {
            List<PredicateExp> returnList = new List<PredicateExp>();
            if (argIndex >= parentPredicate.Arguments.Count)
                return returnList;

            foreach (var obj in objDict[parentPredicate.Arguments[argIndex].Type.Name])
            {
                var newPredicate = new PredicateExp(null, parentPredicate.Name, parentPredicate.Arguments.Copy());
                if (argIndex == parentPredicate.Arguments.Count - 1)
                    returnList.Add(newPredicate);
                newPredicate.Arguments.RemoveAt(argIndex);
                newPredicate.Arguments.Insert(argIndex, new NameExp(null, obj));

                returnList.AddRange(GeneratePossibles(newPredicate, argIndex + 1, objDict));
            }

            return returnList;
        }

        private Dictionary<string, List<string>> DictionaryObjectsOfType(List<NameExp> objs)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();

            foreach (var obj in objs)
            {
                if (!dict.ContainsKey(obj.Type.Name))
                    dict.Add(obj.Type.Name, new List<string>());
                dict[obj.Type.Name].Add(obj.Name);
            }

            return dict;
        }
    }
}
