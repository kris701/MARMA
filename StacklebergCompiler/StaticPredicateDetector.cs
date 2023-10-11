using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StacklebergCompiler
{
    public static class StaticPredicateDetector
    {
        private static List<string> _staticPredicates = new List<string>();
        public static List<string> StaticPredicates => _staticPredicates;
        public static List<string> GenerateStaticPredicates(DomainDecl domain)
        {
            _staticPredicates = new List<string>();

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
            foreach(var action in domain.Actions)
            {
                var allPredicates = action.Effects.FindTypes<PredicateExp>();
                if (allPredicates.Contains(predicate))
                    return true;
            }
            return false;
        }
    }
}
