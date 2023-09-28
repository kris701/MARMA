using PDDLSharp.Models.AST;
using PDDLSharp.Models.Domain;
using PDDLSharp.Models.Problem;
using PDDLSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDDLSharp.Models.Expressions;
using Tools;

namespace StacklebergCompiler
{
    public class LeaderFollowerCompiler
    {
        public PDDLDecl GeneratePrimaryLeaderFollowerDecl(DomainDecl domain, ProblemDecl problem)
        {
            var leaderDomain = domain.Copy();
            var leaderProblem = problem.Copy();
            var followerDomain = domain.Copy();
            var followerProblem = problem.Copy();

            PrefixActionNames(leaderDomain, "fix_");
            PrefixPredicatesNames(leaderDomain, "fix_");
            PrefixPredicatesNames(leaderProblem, "fix_");
            PrefixObjectNames(leaderProblem, "fix_");

            PrefixActionNames(followerDomain, "attack_");
            PrefixPredicatesNames(followerDomain, "attack_");
            PrefixPredicatesNames(followerProblem, "attack_");
            PrefixObjectNames(followerProblem, "attack_");

            var newDecl = new PDDLDecl(Merge(leaderDomain, followerDomain), Merge(leaderProblem, followerProblem));

            newDecl.Problem.Goal.GoalExp = new AndExp(new ASTNode(), newDecl.Problem, new List<IExp>());

            return newDecl;
        }

        private void PrefixActionNames(DomainDecl domain, string prefix)
        {
            foreach (var action in domain.Actions)
                action.Name = $"{prefix}{action.Name}";
        }

        private void PrefixPredicatesNames(DomainDecl domain, string prefix)
        {
            var allPredicates = domain.FindTypes<PredicateExp>();
            foreach(var predicate in  allPredicates)
                predicate.Name = $"{prefix}{predicate.Name}";
        }

        private void PrefixPredicatesNames(ProblemDecl problem, string prefix)
        {
            var allPredicates = problem.FindTypes<PredicateExp>();
            foreach (var predicate in allPredicates)
                predicate.Name = $"{prefix}{predicate.Name}";
        }

        private void PrefixObjectNames(ProblemDecl problem, string prefix)
        {
            var allNames = problem.FindTypes<NameExp>();
            foreach (var name in allNames)
                name.Name = $"{prefix}{name.Name}";
        }

        private DomainDecl Merge(DomainDecl domain1, DomainDecl domain2)
        {
            if (domain2.Predicates != null && domain1.Predicates != null)
                foreach (var predicate in domain2.Predicates.Predicates)
                    domain1.Predicates.Predicates.Add(predicate);
            foreach (var action in domain2.Actions)
                domain1.Actions.Add(action);

            return domain1;
        }

        private ProblemDecl Merge(ProblemDecl problem1, ProblemDecl problem2)
        {
            if (problem2.Objects != null && problem1.Objects != null)
                foreach (var obj in problem2.Objects.Objs)
                    problem1.Objects.Objs.Add(obj);
            foreach (var init in problem2.Init.Predicates)
                problem1.Init.Predicates.Add(init);

            return problem1;
        }
    }
}
