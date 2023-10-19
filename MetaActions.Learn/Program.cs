using CommandLine;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        private static string _tempProblemPath = "problems";
        private static string _tempMacroPath = "macros";
        private static string _tempMacroTempPath = "macrosTemp";
        private static string _tempMetaActionPath = "metaActions";
        private static string _tempCompiledPath = "compiled";
        private static string _tempVerificationPath = "verification";
        private static string _outValidMetaActionPath = "valid";

        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        private static void Run(Options opts)
        {
            ConsoleHelper.WriteLineColor($"Starting to learn meta actions...", ConsoleColor.Blue);

            opts.DomainPath = PathHelper.RootPath(opts.DomainPath);
            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            RecratePath(opts.TempPath);
            RecratePath(opts.OutputPath);

            _tempProblemPath = Path.Combine(opts.TempPath, _tempProblemPath);
            _tempMacroPath = Path.Combine(opts.TempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(opts.TempPath, _tempMacroTempPath);
            _tempMetaActionPath = Path.Combine(opts.TempPath, _tempMetaActionPath);
            _tempCompiledPath = Path.Combine(opts.TempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(opts.TempPath, _tempVerificationPath);
            _outValidMetaActionPath = Path.Combine(opts.OutputPath, _outValidMetaActionPath);

            RecratePath(_tempProblemPath);
            RecratePath(_tempMacroPath);
            RecratePath(_tempMacroTempPath);
            RecratePath(_tempMetaActionPath);
            RecratePath(_tempCompiledPath);
            RecratePath(_tempVerificationPath);
            RecratePath(_outValidMetaActionPath);

            var problems = CopyProblemsToTemp(opts.Problems);

            ConsoleHelper.WriteLineColor($"There is a total of {problems.Count} problems to train with.", ConsoleColor.Blue);

            ConsoleHelper.WriteLineColor($"Generating macros", ConsoleColor.Blue);
            List<FileInfo> allMacros = GenerateMacros(opts.DomainPath);
            ConsoleHelper.WriteLineColor($"A total of {allMacros.Count} macros was found.", ConsoleColor.Blue);
            if (allMacros.Count == 0)
                return;

            ConsoleHelper.WriteLineColor($"Generating meta actions", ConsoleColor.Blue);
            List<FileInfo> allMetaActions = GenerateMetaActions(opts.DomainPath);
            ConsoleHelper.WriteLineColor($"A total of {allMetaActions.Count} meta actions was found.", ConsoleColor.Blue);
            if (allMetaActions.Count == 0)
                return;

            ConsoleHelper.WriteLineColor($"Testing meta actions", ConsoleColor.Blue);
            int totalValidMetaActions = 0;
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                ConsoleHelper.WriteLineColor($"\tTesting meta action {metaActionCounter} of {allMetaActions.Count} [{Math.Round(((double)metaActionCounter / (double)allMetaActions.Count) * 100, 0)}%]", ConsoleColor.Magenta);
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in problems)
                {
                    ConsoleHelper.WriteLineColor($"\t\tProblem {problemCounter} out of {problems.Count} [{Math.Round(((double)problemCounter / (double)problems.Count) * 100, 0)}%].", ConsoleColor.DarkMagenta);
                    // Compile Meta Actions
                    ConsoleHelper.WriteLineColor($"\t\tCompiling meta action.", ConsoleColor.DarkMagenta);
                    CompileMetaAction(opts.DomainPath, problem.FullName, metaAction.FullName);

                    // Verify Meta Actions
                    ConsoleHelper.WriteLineColor($"\t\tVerifying meta action.", ConsoleColor.DarkMagenta);
                    var isMetaActionValid = VerifyMetaAction();

                    // Stop if invalid
                    if (!isMetaActionValid)
                    {
                        ConsoleHelper.WriteLineColor($"\tMeta action was invalid in problem '{problem.Name}'.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    problemCounter++;
                }
                if (allValid)
                {
                    ConsoleHelper.WriteLineColor($"\tMeta action was valid in all {problems.Count} problems.", ConsoleColor.Green);
                    totalValidMetaActions++;
                    File.Copy(metaAction.FullName, Path.Combine(_outValidMetaActionPath, $"meta{metaActionCounter}.pddl"));
                }
                metaActionCounter++;
            }
            ConsoleHelper.WriteLineColor($"A total of {totalValidMetaActions} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green);
        }

        private static void RecratePath(string path)
        {
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
            Directory.CreateDirectory(path);
        }

        private static List<FileInfo> CopyProblemsToTemp(IEnumerable<string> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                var file = new FileInfo(PathHelper.RootPath(problem));
                File.Copy(file.FullName, Path.Combine(_tempProblemPath, file.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, file.Name)));
            }
            return problems;
        }

        private static List<FileInfo> GenerateMacros(string domain)
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

        private static List<FileInfo> GenerateMetaActions(string domain)
        {
            ArgsCaller metaCaller = ArgsCallerBuilder.GetDotnetRunner("MetaActionGenerator");
            metaCaller.Arguments.Add("--domain", domain);
            metaCaller.Arguments.Add("--macros", _tempMacroPath);
            metaCaller.Arguments.Add("--output", _tempMetaActionPath);
            if (metaCaller.Run() != 0)
                throw new Exception("Meta action generation failed!");
            return new DirectoryInfo(_tempMetaActionPath).GetFiles().ToList();
        }

        private static void CompileMetaAction(string domain, string problem, string metaAction)
        {
            ArgsCaller stackelCompiler = ArgsCallerBuilder.GetDotnetRunner("StacklebergCompiler");
            stackelCompiler.Arguments.Add("--domain", domain);
            stackelCompiler.Arguments.Add("--problem", problem);
            stackelCompiler.Arguments.Add("--meta-action", metaAction);
            stackelCompiler.Arguments.Add("--output", _tempCompiledPath);
            if (stackelCompiler.Run() != 0)
                throw new Exception("Stackelberg Compiler failed!");
        }

        private static bool VerifyMetaAction()
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