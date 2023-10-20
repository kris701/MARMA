using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Toolkit.Grounders;

namespace StacklebergCompiler
{
    public static class TotalGoalGenerator
    {
        public static List<PredicateExp> TotalGoal = new List<PredicateExp>();
        public static List<PredicateExp> CopyTotalGoal()
        {
            List<PredicateExp> retList = new List<PredicateExp>();
            foreach (var pred in TotalGoal)
                retList.Add(pred.Copy(null));
            return retList;
        }
        public static void GenerateTotalGoal(ProblemDecl problem, DomainDecl domain)
        {
            IGrounder<PredicateExp, PredicateExp> grounder = new PredicateGrounder(new PDDLDecl(domain, problem));
            if (domain.Predicates != null) 
            {
                List<PredicateExp> newGoals = new List<PredicateExp>();
                foreach (var predicate in domain.Predicates.Predicates)
                    if (!StaticPredicateDetector.StaticPredicates.Contains(predicate.Name))
                        newGoals.AddRange(grounder.Ground(predicate));
                foreach (var predicate in newGoals)
                    predicate.Name = $"{ReservedNames.IsGoalPrefix}{predicate.Name}";
                TotalGoal = newGoals;
            }
        }
    }
}
