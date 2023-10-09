using PDDLSharp.Contextualisers;
using PDDLSharp.Models;
using PDDLSharp.Models.AST;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
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
            TotalGoalGenerator.GenerateTotalGoal(newProblem, newDomain);

            // Problem
            GenerateNewInits(newProblem);
            GenerateNewGoal(newProblem);
            InsertTurnPredicateIntoInit(newProblem);

            // Domain
            SplitActionsIntoLeaderFollowerVariants(newDomain);
            InsertAndIntoAllActions(newDomain);
            GenerateDomainPredicates(newDomain);
            UpdateLeaderActionsPredicatesAndEffects(newDomain);
            InsertConditionalEffectsToFollowerActions(newDomain);
            UpdateAndInsertMetaActionToFit(newDomain, metaAction);
            InsertTurnPredicateIntoActions(newDomain);

            return new PDDLDecl(newDomain, newProblem);
        }

        #region Problem

        /// <summary>
        /// Insert the `leader-turn` predicate to the goal init
        /// </summary>
        /// <param name="problem"></param>
        private void InsertTurnPredicateIntoInit(ProblemDecl problem)
        {
            if (problem.Init != null)
                problem.Init.Predicates.Add(
                    new PredicateExp(ReservedNames.LeaderTurnPredicate));
        }

        /// <summary>
        /// Insert leader prefix predicates and copy them into the existing problem inits.
        /// Also fills the init with the total goal predicates.
        /// </summary>
        /// <param name="problem"></param>
        private void GenerateNewInits(ProblemDecl problem)
        {
            if (problem.Init != null)
            {
                var initPredicates = problem.Init.FindTypes<PredicateExp>();
                problem.Init.Predicates.AddRange(GeneratePrefixPredicates(initPredicates, ReservedNames.LeaderStatePrefix));
                problem.Init.Predicates.AddRange(TotalGoalGenerator.TotalGoal.Copy());
            }
        }

        /// <summary>
        /// Replace the existing goal expression with the total goal predicates.
        /// </summary>
        /// <param name="problem"></param>
        private void GenerateNewGoal(ProblemDecl problem)
        {
            if (problem.Goal != null)
            {
                var goals = TotalGoalGenerator.TotalGoal.Copy();
                List<IExp> newGoals = new List<IExp>();
                foreach (var goal in goals)
                    newGoals.Add(goal);

                problem.Goal.GoalExp = new AndExp(newGoals);
            }
        }

        #endregion

        #region Domain

        /// <summary>
        /// Convert all actions' Preconditions and Effects into having a root 'AndExp' (this makes some things easier later on)
        /// </summary>
        /// <param name="domain"></param>
        private void InsertAndIntoAllActions(DomainDecl domain)
        {
            foreach (var act in domain.Actions)
            {
                if (act.Effects is not AndExp)
                    act.Effects = new AndExp(new List<IExp>()
                    {
                        act.Effects
                    });
                if (act.Preconditions is not AndExp)
                    act.Preconditions = new AndExp(new List<IExp>()
                    {
                        act.Preconditions
                    });
            }
        }

        private void InsertTurnPredicateIntoActions(DomainDecl domain)
        {
            if (domain.Predicates != null)
                domain.Predicates.Predicates.Add(
                    new PredicateExp(ReservedNames.LeaderTurnPredicate));

            foreach(var action in domain.Actions)
            {
                if (action.Name.Contains(ReservedNames.LeaderActionPrefix))
                {
                    if (action.Preconditions is AndExp leaderPreconditions)
                        leaderPreconditions.Children.Add(new PredicateExp(ReservedNames.LeaderTurnPredicate));
                }
                else if (action.Name.Contains(ReservedNames.FollowerActionPrefix))
                {
                    if (action.Preconditions is AndExp leaderPreconditions)
                        leaderPreconditions.Children.Add(new NotExp(new PredicateExp(ReservedNames.LeaderTurnPredicate)));
                }
            }
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
                        newChildren.Add(new NotExp(newNormalEffect));
                    }
                    else
                    {
                        var newNormalEffect = effect.Copy();
                        newNormalEffect.Name = $"{ReservedNames.LeaderStatePrefix}{newNormalEffect.Name}";
                        newChildren.Add(newNormalEffect);
                    }

                    var newGoalEffect = effect.Copy();
                    newGoalEffect.Name = $"{ReservedNames.IsGoalPrefix}{newGoalEffect.Name}";
                    newChildren.Add(new NotExp(newGoalEffect));
                }
            }
            newChildren.Add(new NotExp(new PredicateExp(ReservedNames.LeaderTurnPredicate)));
            metaAction.Effects = new AndExp(newChildren);

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
                action.Effects = new AndExp(newChildren);
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
                        falseWhen.Effect = new NotExp(new PredicateExp(
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
                        falseWhen.Condition = new NotExp(new PredicateExp(
                            falseWhen,
                            $"{ReservedNames.LeaderStatePrefix}{pred.Name}",
                            pred.Arguments));
                        falseWhen.Effect = new NotExp(new PredicateExp(
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

        #endregion

        /// <summary>
        /// Takes a list of predicates, and "copies" them but with a prefix.
        /// </summary>
        /// <param name="predicates"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        private List<PredicateExp> GeneratePrefixPredicates(List<PredicateExp> predicates, string prefix)
        {
            var newPredicates = new List<PredicateExp>();
            foreach (var predicate in predicates)
            {
                if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                    newPredicates.Add(new PredicateExp(
                        $"{prefix}{predicate.Name}",
                        predicate.Arguments));
            }
            return newPredicates;
        }
    }
}
