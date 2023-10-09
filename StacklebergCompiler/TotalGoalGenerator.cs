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
    public static class TotalGoalGenerator
    {
        public static List<PredicateExp> TotalGoal = new List<PredicateExp>();
        public static void GenerateTotalGoal(ProblemDecl problem, DomainDecl domain)
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
                TotalGoal = newGoals;
            }
        }

        private static List<PredicateExp> GeneratePossibles(PredicateExp parentPredicate, int argIndex, Dictionary<string, List<string>> objDict)
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

        private static Dictionary<string, List<string>> DictionaryObjectsOfType(List<NameExp> objs)
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
