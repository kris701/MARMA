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
        private readonly string LeaderPrefix = "attack_";
        private readonly string FollowerPrefix = "fix_";

        public PDDLDecl GenerateConditionalEffects(DomainDecl domain, ProblemDecl problem, ActionDecl metaAction)
        {
            var newDomain = domain.Copy();
            var newProblem = problem.Copy();

            // Domain
            GenerateLeaderFollowerActions(newDomain);
            if (newDomain.Predicates != null)
            {
                var newPredicates = new List<PredicateExp>();
                newPredicates.AddRange(GenerateLeaderPredicates(newDomain));
                newPredicates.AddRange(GenerateGoalPredicates(newDomain));
                newDomain.Predicates.Predicates.AddRange(newPredicates);
            }
            UpdateLeaderActionsPredicatePrefixes(newDomain);
            UpdateFollowerActionsEffects(newDomain);

            // Problem
            GenerateLeaderInits(newProblem);
            GenerateNewGoal(newProblem);

            return new PDDLDecl(newDomain, newProblem);
        }

        private void UpdateLeaderActionsPredicatePrefixes(DomainDecl domain)
        {
            foreach (var action in domain.Actions.Where(x => x.Name.StartsWith(LeaderPrefix)))
            {
                var predicates = action.FindTypes<PredicateExp>();
                foreach (var predicate in predicates)
                    predicate.Name = $"leader-state-{predicate.Name}";
            }
        }

        private void UpdateFollowerActionsEffects(DomainDecl domain)
        {
            foreach (var action in domain.Actions.Where(x => x.Name.StartsWith(FollowerPrefix)))
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
                            $"leader-state-{pred.Name}",
                            pred.Arguments));
                        newWhen.Effect = new PredicateExp(
                            newWhen,
                            $"is-goal-{pred.Name}",
                            pred.Arguments);
                    }
                    else
                    {
                        newWhen.Condition = new PredicateExp(
                            newWhen,
                            $"leader-state-{pred.Name}",
                            pred.Arguments);
                        newWhen.Effect = new PredicateExp(
                            newWhen,
                            $"is-goal-{pred.Name}",
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
                        $"leader-state-{predicate.Name}",
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
                        $"is-goal-{predicate.Name}",
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
                leaderAct.Name = $"{LeaderPrefix}{leaderAct.Name}";
                var followerAct = action.Copy();
                followerAct.Name = $"{FollowerPrefix}{followerAct.Name}";
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
                        $"{LeaderPrefix}{pred.Name}",
                        pred.Arguments));
            }
            problem.Init.Predicates.AddRange(newInits);
        }

        private void GenerateNewGoal(ProblemDecl problem)
        {
            var predicates = problem.Goal.GoalExp.FindTypes<PredicateExp>();
            foreach (var predicate in predicates)
                predicate.Name = $"is-goal-{predicate.Name}";
        }
    }
}
