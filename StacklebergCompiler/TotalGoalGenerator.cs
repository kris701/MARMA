using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;

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
            if (problem.Goal != null && problem.Init != null && problem.Objects != null && domain.Predicates != null)
            {
                List<PredicateExp> newGoals = new List<PredicateExp>();
                var objDict = DictionaryObjectsOfType(problem.Objects.Objs);
                if (!objDict.ContainsKey("object"))
                    objDict.Add("object", new List<string>());

                if (domain.Types != null)
                    foreach (var type in domain.Types.Types)
                        if (!objDict.ContainsKey(type.Name))
                            objDict.Add(type.Name, new List<string>());

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
                var newPredicate = parentPredicate.Copy(null);
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
