using MetaActions.Train.Tools;
using Tools;
using System.Timers;
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

namespace MetaActions.Train.Trainers
{
    public class PDDLSharpTrainer : BaseTrainer
    {
        public PDDLSharpTrainer(string domainName, FileInfo domain, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, TimeSpan timeLimit, string tempPath, string outPath, bool usefuls) : base(domainName, domain, trainingProblems, testingProblems, timeLimit, tempPath, outPath, usefuls)
        {
        }

        public override RunReport? Run()
        {
            Print($"Training started (PDDLSharp Macros)", ConsoleColor.Blue);
            var timer = new System.Timers.Timer();
            timer.Interval = TimeLimit.TotalMilliseconds;
            timer.AutoReset = false;
            timer.Elapsed += (s, e) => {
                CancellationToken.Cancel();
            };
            timer.Start();

            Print($"Copying training problems...", ConsoleColor.Blue);
            var problems = CopyProblemsToTemp(TrainingProblems);
            Print($"There is a total of {problems.Count} problems to train with.", ConsoleColor.Blue);

            var allMacros = GetPDDLSharpMacros(Domain, problems, 10);
            if (allMacros.Count == 0)
                return null;
            if (CancellationToken.IsCancellationRequested)
                return null;

            Print($"Generating meta actions", ConsoleColor.Blue);
            var allMetaActions = GenerateMetaActions();
            allMetaActions.Shuffle();
            Print($"A total of {allMetaActions.Count} meta actions was found.", ConsoleColor.Blue);
            if (allMetaActions.Count == 0)
                return null;
            if (CancellationToken.IsCancellationRequested)
                return null;

            Print($"Validating meta actions", ConsoleColor.Blue);
            int metaActionCounter = 1;
            int validMetaActionCount = 0;
            foreach (var metaAction in allMetaActions)
            {
                if (CancellationToken.IsCancellationRequested)
                    return null;
                PathHelper.RecratePath(_tempReplacementsPath);
                Print($"\tTesting meta action {metaActionCounter} of {allMetaActions.Count} [{Math.Round(((double)metaActionCounter / (double)allMetaActions.Count) * 100, 0)}%]", ConsoleColor.Magenta);
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in problems)
                {
                    if (CancellationToken.IsCancellationRequested)
                        return null;
                    Print($"\t\tProblem {problemCounter} out of {problems.Count} [{Math.Round(((double)problemCounter / (double)problems.Count) * 100, 0)}%].", ConsoleColor.DarkMagenta);
                    // Compile Meta Actions
                    Print($"\t\tCompiling meta action.", ConsoleColor.DarkMagenta);
                    CompileMetaAction(Domain.FullName, problem.FullName, metaAction.FullName);

                    // Verify Meta Actions
                    Print($"\t\tVerifying meta action.", ConsoleColor.DarkMagenta);
                    var isMetaActionValid = VerifyMetaAction();

                    // Stop if invalid
                    if (!isMetaActionValid)
                    {
                        Print($"\tMeta action was invalid in problem '{problem.Name}'.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    problemCounter++;
                }
                if (allValid)
                {
                    validMetaActionCount++;
                    Print($"\tMeta action was valid in all {problems.Count} problems.", ConsoleColor.Green);

                    if (OnlyUsefuls)
                    {
                        Print($"\tGenerating initial meta domain...", ConsoleColor.Magenta);
                        GenerateMetaDomain(Domain, new List<FileInfo>() { metaAction }, OutPath, TempPath);

                        Print("\tChecking for meta action usefulness...", ConsoleColor.Magenta);
                        if (!IsMetaActionUseful(metaAction, problems, TempPath))
                            continue;
                        Print("\tMeta Action is Useful!", ConsoleColor.Green);
                    }

                    _currentMetaActions.Add(metaAction);
                    Print($"\tExtracting macros from plans...", ConsoleColor.Magenta);

                    ExtractMacrosFromPlans(Domain, _tempReplacementsPath, _outCache);
                }
                metaActionCounter++;
            }
            Print($"A total of {_currentMetaActions.Count} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green);

            Print($"Generating final meta domain...", ConsoleColor.Blue);
            GenerateMetaDomain(Domain, _currentMetaActions, OutPath, TempPath);

            return new RunReport(DomainName, allMacros.Count, allMetaActions.Count, validMetaActionCount, _currentMetaActions.Count);
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
    }
}
