using MetaActions.Train.Tools;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.FastDownward.Plans;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Parsers.FastDownward.Plans;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Parsers;
using PDDLSharp.Toolkit.MacroGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.MetaActionStrategies
{
    public class PDDLSharpMacros : BaseCancelable, IMetaActionStrategy
    {
        internal string _tempMetaActionPath = "metaActions";
        internal string _tempMacroPath = "macros";
        internal string _tempMacroGeneratorPath = "macrosGeneratorTemp";

        internal string _macroCachePath = "cache/macros";

        public PDDLSharpMacros(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, token)
        {
            _tempMacroPath = Path.Combine(tempPath, _tempMacroPath);
            _tempMacroGeneratorPath = Path.Combine(tempPath, _tempMacroGeneratorPath);
            _tempMetaActionPath = Path.Combine(tempPath, _tempMetaActionPath);

            _macroCachePath = PathHelper.RootPath(_macroCachePath);

            PathHelper.RecratePath(_tempMetaActionPath);
            PathHelper.RecratePath(_tempMacroPath);
            PathHelper.RecratePath(_tempMacroGeneratorPath);
        }

        public List<FileInfo> GetMetaActions(FileInfo domain, List<FileInfo> trainingProblems)
        {
            var allMacros = GetPDDLSharpMacros(domain, trainingProblems, 10);
            if (allMacros.Count == 0)
            {
                Print($"No macros was found for the domain.", ConsoleColor.Red);
                CancellationToken.Cancel();
                return new List<FileInfo>();
            }
            if (CancellationToken.IsCancellationRequested)
                return new List<FileInfo>();

            Print($"Generating meta actions", ConsoleColor.Blue);
            var allMetaActions = GenerateMetaActions();
            allMetaActions.Shuffle();
            Print($"A total of {allMetaActions.Count} meta actions was found.", ConsoleColor.Blue);
            if (allMetaActions.Count == 0)
            {
                Print($"No meta actions was found for the domain.", ConsoleColor.Red);
                CancellationToken.Cancel();
                return new List<FileInfo>();
            }
            if (CancellationToken.IsCancellationRequested)
                return new List<FileInfo>();
            return allMetaActions;
        }

        private List<FileInfo> GetPDDLSharpMacros(FileInfo domain, List<FileInfo> problems, int macroLimit)
        {
            var checkPath = Path.Combine(_macroCachePath, RunID.ToString());
            if (Directory.Exists(checkPath) && RunID != -1)
            {
                Print($"Using macros from cache '{RunID}'", ConsoleColor.Yellow);
                IOHelper.CopyFilesRecursively(checkPath, _tempMacroPath);
            }
            else
            {
                Print($"Generating macros with PDDLSharp", ConsoleColor.Blue);
                IErrorListener listener = new ErrorListener();
                IParser<INode> parser = new PDDLParser(listener);
                var planParser = new FDPlanParser(listener);
                var plans = new List<ActionPlan>();
                foreach (var problem in problems)
                {
                    using (ArgsCaller fdCaller = new ArgsCaller("python3"))
                    {
                        fdCaller.StdOut += (s, o) => { };
                        fdCaller.StdErr += (s, o) => { };
                        fdCaller.Arguments.Add(PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"), "");
                        fdCaller.Arguments.Add("--alias", "lama-first");
                        fdCaller.Arguments.Add("--overall-time-limit", "5m");
                        fdCaller.Arguments.Add("--plan-file", "plan.plan");
                        fdCaller.Arguments.Add(domain.FullName, "");
                        fdCaller.Arguments.Add(problem.FullName, "");
                        fdCaller.Process.StartInfo.WorkingDirectory = _tempMacroGeneratorPath;
                        if (fdCaller.Run() == 0)
                            plans.Add(planParser.Parse(new FileInfo(Path.Combine(_tempMacroGeneratorPath, "plan.plan"))));
                    }
                }

                var domainDecl = parser.ParseAs<DomainDecl>(domain);
                var macroGenerator = new SequentialMacroGenerator(new PDDLDecl(domainDecl, new ProblemDecl()));
                var macros = macroGenerator.FindMacros(plans, macroLimit);
                var codeGenerator = new PDDLCodeGenerator(listener);
                int counter = 0;
                foreach (var macro in macros)
                {
                    var newMacro = Path.Combine(_tempMacroPath, $"{counter++}.pddl");
                    codeGenerator.Generate(macro, newMacro);
                }
                IOHelper.CopyFilesRecursively(_tempMacroPath, checkPath);
            }
            return new DirectoryInfo(_tempMacroPath).GetFiles().ToList();
        }

        private List<FileInfo> GenerateMetaActions()
        {
            ArgsCaller metaCaller = ArgsCallerBuilder.GetDotnetRunner("MetaActionGenerator");
            _activeProcess = metaCaller.Process;
            metaCaller.Arguments.Add("--macros", _tempMacroPath);
            metaCaller.Arguments.Add("--output", _tempMetaActionPath);
            if (metaCaller.Run() != 0 && !CancellationToken.IsCancellationRequested)
            {
                Print("Meta Action Generation failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
            return new DirectoryInfo(_tempMetaActionPath).GetFiles().ToList();
        }
    }
}
