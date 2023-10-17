using CommandLine;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        private static void Run(Options opts)
        {
            opts.DomainPath = PathHelper.RootPath(opts.DomainPath);
            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            if (Directory.Exists(opts.TempPath))
                new DirectoryInfo(opts.TempPath).Delete(true);
            Directory.CreateDirectory(opts.TempPath);
            if (Directory.Exists(opts.OutputPath))
                new DirectoryInfo(opts.OutputPath).Delete(true);
            Directory.CreateDirectory(opts.OutputPath);

            var problemsPath = Path.Combine(opts.TempPath, "problems");
            var problems = new List<FileInfo>();
            Directory.CreateDirectory(problemsPath);
            foreach (var problem in opts.Problems) 
            {
                var file = new FileInfo(PathHelper.RootPath(problem));
                File.Copy(file.FullName, Path.Combine(opts.TempPath, "problems", file.Name));
                problems.Add(new FileInfo(Path.Combine(opts.TempPath, "problems", file.Name)));
            }

            ConsoleHelper.WriteLineColor($"Generating macros", ConsoleColor.Gray);
            Directory.CreateDirectory(Path.Combine(opts.TempPath, "macros"));
            List<FileInfo> allMacros = GenerateMacros(opts.DomainPath, Path.Combine(opts.TempPath, "problems"), opts.TempPath);
            if (Failed)
                return;

            ConsoleHelper.WriteLineColor($"Generating meta actions", ConsoleColor.Gray);
            List<FileInfo> allMetaActions = GenerateMetaActions(opts.DomainPath, opts.TempPath);
            if (Failed)
                return;

            ConsoleHelper.WriteLineColor($"Testing meta actions", ConsoleColor.Gray);
            int totalValidMetaActions = 0;
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                int problemCounter = 1;
                bool allValid = true;
                isValid = false;
                foreach (var problem in problems)
                {
                    ConsoleHelper.WriteLineColor($"Testing meta action {metaActionCounter} of {allMetaActions.Count} on problem {problemCounter++} out of {problems.Count}.", ConsoleColor.Gray);
                    // Compile Meta Actions
                    ConsoleHelper.WriteLineColor($"Compiling meta action.", ConsoleColor.Gray);
                    ArgsCaller stackelCompiler = GetDotnetRunner("StacklebergCompiler");
                    stackelCompiler.Arguments.Add("--domain", opts.DomainPath);
                    stackelCompiler.Arguments.Add("--problem", problem.FullName);
                    stackelCompiler.Arguments.Add("--meta-action", metaAction.FullName);
                    stackelCompiler.Arguments.Add("--output", Path.Combine(opts.TempPath, "compiled"));
                    Failed = false;
                    if (stackelCompiler.Run() != 0)
                        Failed = true;

                    // Verify Meta Actions
                    ConsoleHelper.WriteLineColor($"Verifying meta action.", ConsoleColor.Gray);
                    ArgsCaller stackelVerifier = GetDotnetRunner("StackelbergVerifier");
                    stackelVerifier.StdOut -= PrintStdOut;
                    stackelVerifier.StdOut += PrintStdOutVerifier;
                    stackelVerifier.Arguments.Add("--domain", Path.Combine(opts.TempPath, "compiled", "simplified_domain.pddl"));
                    stackelVerifier.Arguments.Add("--problem", Path.Combine(opts.TempPath, "compiled", "simplified_problem.pddl"));
                    stackelVerifier.Arguments.Add("--output", Path.Combine(opts.TempPath, "verification"));
                    stackelVerifier.Arguments.Add("--stackelberg", "Dependencies/stackelberg-planner/src/fast-downward.py");
                    Failed = false;
                    if (stackelVerifier.Run() != 0)
                        Failed = true;

                    // Output Valid Meta Actions
                    if (!isValid) { 
                        ConsoleHelper.WriteLineColor($"Meta action was invalid.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    isValid = false;
                }
                if (allValid)
                {
                    ConsoleHelper.WriteLineColor($"Meta action was valid.", ConsoleColor.Green);
                    totalValidMetaActions++;
                    if (!Directory.Exists(Path.Combine(opts.OutputPath, "valid")))
                        Directory.CreateDirectory(Path.Combine(opts.OutputPath, "valid"));
                    File.WriteAllText(Path.Combine(opts.OutputPath, "valid", $"meta{metaActionCounter}.pddl"), File.ReadAllText(metaAction.FullName));
                }
                metaActionCounter++;
            }
            ConsoleHelper.WriteLineColor($"A total of {totalValidMetaActions} valid meta actions was found.", ConsoleColor.Green);
        }

        private static List<FileInfo> GenerateMacros(string domain, string problems, string tempPath)
        {
            var macroGenerator = GetRustRunner("macros");
            macroGenerator.Arguments.Add("-d", domain);
            macroGenerator.Arguments.Add("-p", problems);
            macroGenerator.Arguments.Add("-o", Path.Combine(tempPath, "macros"));
            macroGenerator.Arguments.Add("-c", PathHelper.RootPath("Dependencies/CSMs"));
            macroGenerator.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
            Failed = false;
            if (macroGenerator.Run() != 0)
                Failed = true;
            return new DirectoryInfo(Path.Combine(tempPath, "macros")).GetFiles().ToList();
        }

        private static List<FileInfo> GenerateMetaActions(string domain, string tempPath)
        {
            ArgsCaller metaCaller = GetDotnetRunner("MetaActionGenerator");
            metaCaller.Arguments.Add("--domain", domain);
            metaCaller.Arguments.Add("--macros", Path.Combine(tempPath, "macros"));
            metaCaller.Arguments.Add("--output", Path.Combine(tempPath, "metaActions"));
            Failed = false;
            if (metaCaller.Run() != 0)
                Failed = true;
            return new DirectoryInfo(Path.Combine(tempPath, "metaActions")).GetFiles().ToList();
        }

        private static ArgsCaller GetDotnetRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller("dotnet");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("run", "");
            runner.Arguments.Add("--no-restore", "");
            runner.Arguments.Add("--no-build", "");
            runner.Arguments.Add("--project", $"{project} --");
            return runner;
        }

        private static ArgsCaller GetRustRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller("cargo");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("run", "");
            runner.Arguments.Add("--release", "");
            runner.Arguments.Add("--manifest-path", $"{project}/Cargo.toml --");
            return runner;
        }

        private static bool isValid = false;
        private static void PrintStdOutVerifier(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (e.Data.Contains("== Frontier is valid =="))
                    isValid = true;
#if DEBUG
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Gray);
#endif
            }
        }

        private static void PrintStdOut(object sender, DataReceivedEventArgs e)
        {
#if DEBUG
            if (e.Data != null)
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Gray);
#endif
        }

        private static bool Failed = false;
        private static void PrintStdErr(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Failed = true;
#if DEBUG
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Red);
#endif
            }
        }
    }
}