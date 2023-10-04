using PDDLSharp.Models;
using PDDLSharp.Models.Domain;
using PDDLSharp.Models.Expressions;
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
                        if (IsInEffects(predicate.Parent, predicate))
                            isStatic = false;
                    }
                    if (isStatic)
                        _staticPredicates.Add(currentName);
                }
            }

            return _staticPredicates;
        }

        private static bool IsInEffects(INode node, INode target)
        {
            if (node.Parent != null)
            {
                if (node.Parent is ActionDecl act)
                {
                    if (act.Effects is AndExp and)
                        foreach (var innerNode in and)
                            if (innerNode == target)
                                return true;
                }
                else
                    return IsInEffects(node.Parent, target);
            }
            return false;
        }
    }
}
