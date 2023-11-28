using MetaActions.Train.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.MetaActionStrategies
{
    public class CSMMacros : BaseCancelable, IMetaActionStrategy
    {
        internal string _tempProblemPath = "problems";
        internal string _tempMetaActionPath = "metaActions";
        internal string _tempMacroPath = "macros";
        internal string _tempMacroTempPath = "macrosTemp";
        internal string _tempMacroGeneratorPath = "macrosGeneratorTemp";

        internal string _macroCachePath = "cache/macros";

        public CSMMacros(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, token)
        {
            _tempMacroPath = Path.Combine(tempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(tempPath, _tempMacroTempPath);
            _tempMacroGeneratorPath = Path.Combine(tempPath, _tempMacroGeneratorPath);
            _tempMetaActionPath = Path.Combine(tempPath, _tempMetaActionPath);
            _tempProblemPath = Path.Combine(tempPath, _tempProblemPath);

            _macroCachePath = PathHelper.RootPath(_macroCachePath);

            PathHelper.RecratePath(_tempMetaActionPath);
            PathHelper.RecratePath(_tempProblemPath);
            PathHelper.RecratePath(_tempMacroPath);
            PathHelper.RecratePath(_tempMacroTempPath);
            PathHelper.RecratePath(_tempMacroGeneratorPath);
        }

        public List<FileInfo> GetMetaActions(FileInfo domain, List<FileInfo> trainingProblems)
        {
            Print($"Generating macros using CSM", ConsoleColor.Blue);

            Print($"Copying training problems...", ConsoleColor.Blue);
            CopyProblemsToTemp(trainingProblems);
            if (CancellationToken.IsCancellationRequested) return new List<FileInfo>();

            var allMacros = GetCSMMacros(domain);
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

        private void CopyProblemsToTemp(List<FileInfo> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                if (CancellationToken.IsCancellationRequested) return;
                File.Copy(problem.FullName, Path.Combine(_tempProblemPath, problem.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, problem.Name)));
            }
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
                _activeProcess = macroGenerator.Process;
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
