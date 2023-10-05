using PDDLSharp.Contextualisers;
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
            TotalGoalGenerator.GenerateIsGoal(newProblem, newDomain);

            // Problem
            GenerateNewInits(newProblem);
            GenerateNewGoal(newProblem, newDomain);
            InsertTurnPredicateIntoInit(newProblem);

            // Domain
            SplitActionsIntoLeaderFollowerVariants(newDomain);
            TurnAllActionEffectsToAnd(newDomain);
            GenerateDomainPredicates(newDomain);
            UpdateLeaderActionsPredicatesAndEffects(newDomain);
            InsertConditionalEffectsToFollowerActions(newDomain);
            UpdateAndInsertMetaActionToFit(newDomain, metaAction);
            InsertTurnPredicateIntoActions(newDomain);

            return new PDDLDecl(newDomain, newProblem);
        }

        private void TurnAllActionEffectsToAnd(DomainDecl domain)
        {
            foreach (var act in domain.Actions)
                if (act.Effects is not AndExp)
                    act.Effects = new AndExp(null, new List<IExp>()
                    {
                        act.Effects
                    });
        }

        private void InsertTurnPredicateIntoActions(DomainDecl domain)
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

        private void InsertTurnPredicateIntoInit(ProblemDecl problem)
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

        private void UpdateLeaderActionsPredicatesAndEffects(DomainDecl domain)
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

        private void InsertConditionalEffectsToFollowerActions(DomainDecl domain)
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

        private void GenerateDomainPredicates(DomainDecl domain)
        {
            if (domain.Predicates != null)
            {
                var newPredicates = new List<PredicateExp>();
                newPredicates.AddRange(GeneratePrefixPredicates(domain.Predicates.Predicates, ReservedNames.LeaderStatePrefix));
                newPredicates.AddRange(GeneratePrefixPredicates(domain.Predicates.Predicates, ReservedNames.IsGoalPrefix));
                domain.Predicates.Predicates.AddRange(newPredicates);
            }
        }

        private List<PredicateExp> GeneratePrefixPredicates(List<PredicateExp> predicates, string prefix)
        {
            var newLeaderPredicates = new List<PredicateExp>();
            foreach (var predicate in predicates)
            {
                if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                    newLeaderPredicates.Add(new PredicateExp(
                        null,
                        $"{prefix}{predicate.Name}",
                        predicate.Arguments));
            }
            return newLeaderPredicates;
        }

        private void SplitActionsIntoLeaderFollowerVariants(DomainDecl domain)
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
                var initPredicates = problem.Init.FindTypes<PredicateExp>();
                problem.Init.Predicates.AddRange(GeneratePrefixPredicates(initPredicates, ReservedNames.LeaderStatePrefix));
                problem.Init.Predicates.AddRange(TotalGoalGenerator.IsGoal.Copy());
            }
        }

        private void GenerateNewGoal(ProblemDecl problem, DomainDecl domain)
        {
            if (problem.Goal != null && problem.Init != null && problem.Objects != null && domain.Predicates != null)
            {
                var goals = TotalGoalGenerator.IsGoal.Copy();
                List<IExp> newGoals = new List<IExp>();
                foreach (var goal in goals)
                    newGoals.Add(goal);

                problem.Goal.GoalExp = new AndExp(null, newGoals);
            }
        }
    }
}
