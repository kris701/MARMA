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

            // Problem
            GenerateLeaderInits(newProblem);
            GenerateNewGoal(newProblem);
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
            UpdateLeaderActionsPredicatePrefixes(newDomain);
            UpdateFollowerActionsEffects(newDomain);
            UpdateAndInsertMetaActionToFit(newDomain, metaAction);
            InsertTurnPredicate(newDomain);
            AddPassTurnToFollowerAction(newDomain);
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
                goal.GoalExp.Copy()
                ));
        }

        private void AddPassTurnToFollowerAction(DomainDecl domain)
        {
            domain.Actions.Add(new ActionDecl(
                null,
                ReservedNames.LeaderPassTurnAction,
                new ParameterExp(null, new List<NameExp>()),
                new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()),
                new NotExp(null, new PredicateExp(null, ReservedNames.LeaderTurnPredicate, new List<NameExp>()))));
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
            var metaPredicates = metaAction.FindTypes<PredicateExp>();
            foreach (var predicate in metaPredicates)
                predicate.Name = $"{ReservedNames.LeaderStatePrefix}{predicate.Name}";
            metaAction.Name = $"{ReservedNames.LeaderActionPrefix}{ReservedNames.MetaActionPrefix}{metaAction.Name}";
            domain.Actions.Add(metaAction);
        }

        private void UpdateLeaderActionsPredicatePrefixes(DomainDecl domain)
        {
            foreach (var action in domain.Actions.Where(x => x.Name.StartsWith(ReservedNames.LeaderActionPrefix)))
            {
                var predicates = action.FindTypes<PredicateExp>();
                foreach (var predicate in predicates)
                    predicate.Name = $"{ReservedNames.LeaderStatePrefix}{predicate.Name}";
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
                    var newWhen = new WhenExp(action, null, null);
                    if (pred.Parent is NotExp)
                    {
                        newWhen.Condition = new NotExp(newWhen, new PredicateExp(
                            newWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments));
                        newWhen.Effect = new PredicateExp(
                            newWhen,
                            $"{ReservedNames.IsGoalPrefix}{pred.Name}",
                            pred.Arguments);
                    }
                    else
                    {
                        newWhen.Condition = new PredicateExp(
                            newWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments);
                        newWhen.Effect = new PredicateExp(
                            newWhen,
                            $"{ReservedNames.IsGoalPrefix}{pred.Name}",
                            pred.Arguments);
                    }
                    newExpressions.Add(newWhen);
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

        private void GenerateLeaderInits(ProblemDecl problem)
        {
            var newInits = new List<PredicateExp>();
            foreach (var init in problem.Init.Predicates)
            {
                if (init is PredicateExp pred)
                    newInits.Add(new PredicateExp(
                        problem.Init,
                        $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                        pred.Arguments));
            }
            problem.Init.Predicates.AddRange(newInits);
        }

        private void GenerateNewGoal(ProblemDecl problem)
        {
            if (problem.Goal != null)
            {
                //problem.Goal.GoalExp = new AndExp(null, new List<IExp>());
            }
        }
    }
}
