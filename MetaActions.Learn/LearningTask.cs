using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Learn
{
    public class LearningTask
    {
        private string _tempProblemPath = "problems";
        private string _tempMacroPath = "macros";
        private string _tempMacroTempPath = "macrosTemp";
        private string _tempMetaActionPath = "metaActions";
        private string _tempCompiledPath = "compiled";
        private string _tempVerificationPath = "verification";
        private string _outValidMetaActionPath = "valid";

        public bool DebugMode { get; set; } = false;

        public void LearnDomain(string tempPath, string outPath, FileInfo domain, List<FileInfo> sourceProblems)
        {
            RecratePath(tempPath);
            RecratePath(outPath);

            _tempProblemPath = Path.Combine(tempPath, _tempProblemPath);
            _tempMacroPath = Path.Combine(tempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(tempPath, _tempMacroTempPath);
            _tempMetaActionPath = Path.Combine(tempPath, _tempMetaActionPath);
            _tempCompiledPath = Path.Combine(tempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(tempPath, _tempVerificationPath);
            _outValidMetaActionPath = Path.Combine(outPath, _outValidMetaActionPath);

            RecratePath(_tempProblemPath);
            RecratePath(_tempMacroPath);
            RecratePath(_tempMacroTempPath);
            RecratePath(_tempMetaActionPath);
            RecratePath(_tempCompiledPath);
            RecratePath(_tempVerificationPath);
            RecratePath(_outValidMetaActionPath);

            var problems = CopyProblemsToTemp(sourceProblems);

            Print($"There is a total of {problems.Count} problems to train with.", ConsoleColor.Blue);

            Print($"Generating macros", ConsoleColor.Blue);
            List<FileInfo> allMacros = GenerateMacros(domain.FullName);
            Print($"A total of {allMacros.Count} macros was found.", ConsoleColor.Blue);
            if (allMacros.Count == 0)
                return;

            Print($"Generating meta actions", ConsoleColor.Blue);
            List<FileInfo> allMetaActions = GenerateMetaActions(domain.FullName);
            Print($"A total of {allMetaActions.Count} meta actions was found.", ConsoleColor.Blue);
            if (allMetaActions.Count == 0)
                return;

            Print($"Testing meta actions", ConsoleColor.Blue);
            int totalValidMetaActions = 0;
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                Print($"\tTesting meta action {metaActionCounter} of {allMetaActions.Count} [{Math.Round(((double)metaActionCounter / (double)allMetaActions.Count) * 100, 0)}%]", ConsoleColor.Magenta);
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in problems)
                {
                    Print($"\t\tProblem {problemCounter} out of {problems.Count} [{Math.Round(((double)problemCounter / (double)problems.Count) * 100, 0)}%].", ConsoleColor.DarkMagenta);
                    // Compile Meta Actions
                    Print($"\t\tCompiling meta action.", ConsoleColor.DarkMagenta);
                    CompileMetaAction(domain.FullName, problem.FullName, metaAction.FullName);

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
                    Print($"\tMeta action was valid in all {problems.Count} problems.", ConsoleColor.Green);
                    totalValidMetaActions++;
                    File.Copy(metaAction.FullName, Path.Combine(_outValidMetaActionPath, $"meta{metaActionCounter}.pddl"));
                }
                metaActionCounter++;
            }
            Print($"A total of {totalValidMetaActions} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green);
        }

        private void Print(string text, ConsoleColor color)
        {
            if (DebugMode)
                Print(text, color);
        }

        private void RecratePath(string path)
        {
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
            Directory.CreateDirectory(path);
        }

        private List<FileInfo> CopyProblemsToTemp(List<FileInfo> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                File.Copy(problem.FullName, Path.Combine(_tempProblemPath, problem.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, problem.Name)));
            }
            return problems;
        }

        private List<FileInfo> GenerateMacros(string domain)
        {
            var macroGenerator = ArgsCallerBuilder.GetRustRunner("macros");
            macroGenerator.Arguments.Add("-d", domain);
            macroGenerator.Arguments.Add("-p", _tempProblemPath);
            macroGenerator.Arguments.Add("-o", _tempMacroPath);
            macroGenerator.Arguments.Add("-t", _tempMacroTempPath);
            macroGenerator.Arguments.Add("-c", PathHelper.RootPath("Dependencies/CSMs"));
            macroGenerator.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
            if (macroGenerator.Run() != 0)
                throw new Exception("Macro generation failed!");
            return new DirectoryInfo(_tempMacroPath).GetFiles().ToList();
        }

        private List<FileInfo> GenerateMetaActions(string domain)
        {
            ArgsCaller metaCaller = ArgsCallerBuilder.GetDotnetRunner("MetaActionGenerator");
            metaCaller.Arguments.Add("--domain", domain);
            metaCaller.Arguments.Add("--macros", _tempMacroPath);
            metaCaller.Arguments.Add("--output", _tempMetaActionPath);
            if (metaCaller.Run() != 0)
                throw new Exception("Meta action generation failed!");
            return new DirectoryInfo(_tempMetaActionPath).GetFiles().ToList();
        }

        private void CompileMetaAction(string domain, string problem, string metaAction)
        {
            ArgsCaller stackelCompiler = ArgsCallerBuilder.GetDotnetRunner("StacklebergCompiler");
            stackelCompiler.Arguments.Add("--domain", domain);
            stackelCompiler.Arguments.Add("--problem", problem);
            stackelCompiler.Arguments.Add("--meta-action", metaAction);
            stackelCompiler.Arguments.Add("--output", _tempCompiledPath);
            if (stackelCompiler.Run() != 0)
                throw new Exception("Stackelberg Compiler failed!");
        }

        private bool VerifyMetaAction()
        {
            ArgsCaller stackelVerifier = ArgsCallerBuilder.GetDotnetRunner("StackelbergVerifier");
            stackelVerifier.Arguments.Add("--domain", Path.Combine(_tempCompiledPath, "simplified_domain.pddl"));
            stackelVerifier.Arguments.Add("--problem", Path.Combine(_tempCompiledPath, "simplified_problem.pddl"));
            stackelVerifier.Arguments.Add("--output", _tempVerificationPath);
            stackelVerifier.Arguments.Add("--stackelberg", PathHelper.RootPath("Dependencies/stackelberg-planner/src/fast-downward.py"));
            var code = stackelVerifier.Run();
            if (code != 0 && code != 1)
                throw new Exception("Stackelberg verifier failed!");
            return code == 0;
        }
    }
}
