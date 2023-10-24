using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Toolkit.StaticPredicateDetectors;

namespace StackelbergCompiler
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
        }
    }
}
