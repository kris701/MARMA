using MetaActions.Train.Tools;
using Tools;
using System.Timers;

namespace MetaActions.Train.Trainers
{
    public class CSMTrainer : BaseTrainer
    {
        public CSMTrainer(string domainName, FileInfo domain, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, TimeSpan timeLimit, string tempPath, string outPath, bool usefuls, CancellationTokenSource cancellationToken) : base(domainName, domain, trainingProblems, testingProblems, timeLimit, tempPath, outPath, usefuls, cancellationToken)
        {
        }

        public override RunReport? Run()
        {
            Print($"Training started (CSM Macros)", ConsoleColor.Blue);
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

            var allMacros = GetCSMMacros(Domain);
            if (allMacros.Count == 0)
                return null;
            if (CancellationToken.IsCancellationRequested)
                return null;

            Print($"Generating meta actions", ConsoleColor.Blue);
            var allMetaActions = GenerateMetaActions();
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

        private List<FileInfo> GetCSMMacros(FileInfo domain)
        {
            var checkPath = Path.Combine(_macroCachePath, RunID.ToString());
            if (Directory.Exists(checkPath) && RunID != -1)
            {
                Print($"Using macros from cache '{RunID}'", ConsoleColor.Yellow);
                IOHelper.CopyFilesRecursively(checkPath, _tempMacroPath);
            }
            else
            {
                Print($"Copying CSM to temp", ConsoleColor.Blue);
                // Make a temp copy of CSMs, since it cant handle multiple runs at the same time.
                IOHelper.CopyFilesRecursively(PathHelper.RootPath("Dependencies/CSMs/src"), Path.Combine(_tempMacroGeneratorPath, "src"));
                IOHelper.CopyFilesRecursively(PathHelper.RootPath("Dependencies/CSMs/scripts"), Path.Combine(_tempMacroGeneratorPath, "scripts"));

                Print($"Generating macros with CSM", ConsoleColor.Blue);
                var macroGenerator = ArgsCallerBuilder.GetRustRunner("macros");
                macroGenerator.Arguments.Add("-d", domain.FullName);
                macroGenerator.Arguments.Add("-p", _tempProblemPath);
                macroGenerator.Arguments.Add("-o", _tempMacroPath);
                macroGenerator.Arguments.Add("-t", _tempMacroTempPath);
                macroGenerator.Arguments.Add("-c", _tempMacroGeneratorPath);
                macroGenerator.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
                if (macroGenerator.Run() != 0 && !CancellationToken.IsCancellationRequested)
                    throw new Exception("Macro generation failed!");

                PathHelper.RecratePath(checkPath);
                IOHelper.CopyFilesRecursively(_tempMacroPath, checkPath);
            }
            return new DirectoryInfo(_tempMacroPath).GetFiles().ToList();
        }
    }
}
