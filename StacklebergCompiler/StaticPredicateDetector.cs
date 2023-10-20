using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Toolkit.StaticPredicateDetectors;

namespace StacklebergCompiler
{
    public static class StaticPredicateDetector
    {
        private static List<string> _staticPredicates = new List<string>();
        public static List<string> StaticPredicates => _staticPredicates;
        public static List<string> GenerateStaticPredicates(DomainDecl domain)
        {
            _staticPredicates = new List<string>();
            IStaticPredicateDetectors detector = new SimpleStaticPredicateDetector();
            var statics = detector.FindStaticPredicates(new PDDLDecl(domain, new ProblemDecl()));
            foreach (var staticPred in statics)
                _staticPredicates.Add(staticPred.Name);
            return _staticPredicates;

            if (domain.Predicates != null)
            {
                var allPredicates = domain.FindTypes<PredicateExp>();
                allPredicates = allPredicates.OrderBy(x => x.Name).ToList();

                if (allPredicates.Count > 0)
                {
                    string currentName = allPredicates[0].Name;
                    bool isStatic = true;
                    foreach (var predicate in allPredicates)
                    {
                        if (predicate.Name != currentName)
                        {
                            if (isStatic)
                                _staticPredicates.Add(currentName);
                            isStatic = true;
                            currentName = predicate.Name;
                        }
                        if (!isStatic)
                            continue;
                        if (IsInEffects(domain, predicate))
                            isStatic = false;
                    }
                    if (isStatic)
                        _staticPredicates.Add(currentName);
                }
            }

            return _staticPredicates;
        }

        private static bool IsInEffects(DomainDecl domain, PredicateExp predicate)
        {
            foreach (var action in domain.Actions)
            {
                var allPredicates = action.Effects.FindTypes<PredicateExp>();
                if (allPredicates.Contains(predicate))
                    return true;
            }
            return false;
        }
    }
}
