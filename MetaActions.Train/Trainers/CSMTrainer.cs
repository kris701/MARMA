using MetaActions.Train.Tools;
using Tools;

namespace MetaActions.Train.Trainers
{
    public class CSMTrainer : BaseTrainer
    {
        internal string _tempProblemPath = "problems";
        internal string _tempMetaActionPath = "metaActions";
        internal string _tempMacroPath = "macros";
        internal string _tempMacroTempPath = "macrosTemp";
        internal string _tempMacroGeneratorPath = "macrosGeneratorTemp";

        internal string _macroCachePath = "cache/macros";

        public CSMTrainer(string domainName, FileInfo domain, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, TimeSpan timeLimit, string tempPath, string outPath, bool usefuls) : base(domainName, domain, trainingProblems, testingProblems, timeLimit, tempPath, outPath, usefuls)
        {
            _tempMacroPath = Path.Combine(TempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(TempPath, _tempMacroTempPath);
            _tempMacroGeneratorPath = Path.Combine(TempPath, _tempMacroGeneratorPath);
            _tempMetaActionPath = Path.Combine(TempPath, _tempMetaActionPath);
            _tempProblemPath = Path.Combine(TempPath, _tempProblemPath);

            _macroCachePath = PathHelper.RootPath(_macroCachePath);

            PathHelper.RecratePath(_tempMetaActionPath);
            PathHelper.RecratePath(_tempProblemPath);
            PathHelper.RecratePath(_tempMacroPath);
            PathHelper.RecratePath(_tempMacroTempPath);
            PathHelper.RecratePath(_tempMacroGeneratorPath);
        }

        public override List<FileInfo> GetMetaActions()
        {
            Print($"Copying training problems...", ConsoleColor.Blue);
            var problems = CopyProblemsToTemp(TrainingProblems);
            if (CancellationToken.IsCancellationRequested) return new List<FileInfo>();

            var allMacros = GetCSMMacros(Domain);
            if (allMacros.Count == 0)
            {
                Print($"No macros was found for the domain.", ConsoleColor.Red);
                CancellationToken.Cancel();
                return new List<FileInfo>();
            }
            if (CancellationToken.IsCancellationRequested) return new List<FileInfo>();

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
            if (CancellationToken.IsCancellationRequested) return new List<FileInfo>();
            return allMetaActions;
        }

        private List<FileInfo> CopyProblemsToTemp(List<FileInfo> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                if (CancellationToken.IsCancellationRequested)
                    return new List<FileInfo>();
                File.Copy(problem.FullName, Path.Combine(_tempProblemPath, problem.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, problem.Name)));
            }
            return problems;
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
                {
                    Print("Macro Generation failed!", ConsoleColor.Red);
                    CancellationToken.Cancel();
                }

                PathHelper.RecratePath(checkPath);
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
