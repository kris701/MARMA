using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.Contextualisers.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Toolkit.Planners.Heuristics;
using PDDLSharp.Translators.StaticPredicateDetectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.MetaActionStrategies
{
    public class PredicateMetaActions : BaseCancelable, IMetaActionStrategy
    {
        internal string _tempMetaActionPath = "metaActions";
        public PredicateMetaActions(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, token)
        {
            _tempMetaActionPath = Path.Combine(tempPath, _tempMetaActionPath);
            PathHelper.RecratePath(_tempMetaActionPath);
        }

        public List<FileInfo> GetMetaActions(FileInfo domain, List<FileInfo> trainingProblems)
        {
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            var codeGenerator = new PDDLCodeGenerator(listener);
            var contextualiser = new PDDLContextualiser(listener);

            var domainDecl = parser.ParseAs<DomainDecl>(domain);
            var pddlDecl = new PDDLDecl(domainDecl, new ProblemDecl());
            contextualiser.Contexturalise(pddlDecl);

            var statics = SimpleStaticPredicateDetector.FindStaticPredicates(pddlDecl);
            statics.Add(new PredicateExp("="));
            var goals = GetIlligalPredicates(parser, trainingProblems);
            var metaActions = GeneratePredicateMetaActions(domainDecl, statics, goals);

            var result = OutputMetaActions(codeGenerator, metaActions);

            return result;
        }

        private List<string> GetIlligalPredicates(IParser<INode> parser, List<FileInfo> trainingProblems)
        {
            var goals = new List<string>();
            foreach (var problemFile in trainingProblems)
            {
                var problemDecl = parser.ParseAs<ProblemDecl>(problemFile);
                if (problemDecl.Goal != null)
                {
                    var allGoals = problemDecl.Goal.GoalExp.FindTypes<PredicateExp>();
                    foreach (var goal in allGoals)
                        if (!goals.Contains(goal.Name))
                            goals.Add(goal.Name);
                }
            }
            return goals;
        }

        private List<ActionDecl> GeneratePredicateMetaActions(DomainDecl domainDecl, List<PredicateExp> statics, List<string> goals)
        {
            var metaActions = new List<ActionDecl>();
            int metaCounter = 0;
            if (domainDecl.Predicates != null)
            {
                foreach (var predicate in domainDecl.Predicates.Predicates)
                {
                    if (!statics.Any(x => x.Name == predicate.Name) && !goals.Any(x => x == predicate.Name))
                    {
                        metaActions.Add(GenerateActionFromPredicate(predicate, metaCounter++, false));
                        metaActions.Add(GenerateActionFromPredicate(predicate, metaCounter++, true));
                    }
                }
            }
            return metaActions;
        }

        private ActionDecl GenerateActionFromPredicate(PredicateExp predicate, int id, bool isNegativeEffect)
        {
            var genericPredicate = predicate.Copy();
            int argIndex = 0;
            foreach (var arg in genericPredicate.Arguments)
                arg.Name = $"?{argIndex++}";
            var newMetaAction = new ActionDecl($"$meta_{id}");
            foreach (var arg in genericPredicate.Arguments)
                newMetaAction.Parameters.Values.Add(arg);
            if (isNegativeEffect)
                newMetaAction.Effects = new AndExp(new List<IExp>() { new NotExp(genericPredicate) });
            else
                newMetaAction.Effects = new AndExp(new List<IExp>() { genericPredicate });
            return newMetaAction;
        }

        private List<FileInfo> OutputMetaActions(ICodeGenerator<INode> codeGenerator, List<ActionDecl> metaActions)
        {
            var metaActionFiles = new List<FileInfo>();
            foreach (var metaAction in metaActions)
            {
                var target = Path.Combine(_tempMetaActionPath, $"{metaAction.Name}.pddl");
                codeGenerator.Generate(metaAction, target);
                metaActionFiles.Add(new FileInfo(target));
            }
            return metaActionFiles;
        }
    }
}
