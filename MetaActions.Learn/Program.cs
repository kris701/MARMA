using CommandLine;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
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

            ConsoleHelper.WriteLineColor($"Generating macros", ConsoleColor.Gray);
            List<FileInfo> allMacros = GenerateMacros(opts.DomainPath, opts.ProblemsPath, opts.TempPath);
            if (Failed)
                return;

            ConsoleHelper.WriteLineColor($"Generating meta actions", ConsoleColor.Gray);
            List<FileInfo> allMetaActions = GenerateMetaActions(opts.DomainPath, opts.TempPath);
            if (Failed)
                return;

            ConsoleHelper.WriteLineColor($"Testing meta actions", ConsoleColor.Gray);
            int totalValidMetaActions = 0;
            int metaActionCounter = 1;
            var problems = new DirectoryInfo(opts.ProblemsPath).GetFiles().ToList();
            foreach (var metaAction in allMetaActions)
            {
                int problemCounter = 1;
                bool allValid = true;
                isValid = false;
                foreach (var problem in problems)
                {
                    ConsoleHelper.WriteLineColor($"Testing meta action {metaActionCounter} on problem {problemCounter++} out of {problems.Count}.", ConsoleColor.Gray);
                    // Compile Meta Actions
                    ConsoleHelper.WriteLineColor($"Compiling meta action.", ConsoleColor.Gray);
                    ArgsCaller stackelCompiler = GetDotnetRunner("StacklebergCompiler");
                    stackelCompiler.Arguments.Add("--domain", opts.DomainPath);
                    stackelCompiler.Arguments.Add("--problem", problem.FullName);
                    stackelCompiler.Arguments.Add("--meta-action", metaAction.FullName);
                    stackelCompiler.Arguments.Add("--output", Path.Combine(opts.TempPath, "compiled"));
                    stackelCompiler.Run();
                    if (Failed)
                        return;

                    // Verify Meta Actions
                    ConsoleHelper.WriteLineColor($"Verifying meta action.", ConsoleColor.Gray);
                    ArgsCaller stackelVerifier = GetDotnetRunner("StackelbergVerifier");
                    stackelVerifier.StdOut -= PrintStdOut;
                    stackelVerifier.StdOut += PrintStdOutVerifier;
                    stackelVerifier.Arguments.Add("--domain", Path.Combine(opts.TempPath, "compiled", "simplified_domain.pddl"));
                    stackelVerifier.Arguments.Add("--problem", Path.Combine(opts.TempPath, "compiled", "simplified_problem.pddl"));
                    stackelVerifier.Arguments.Add("--output", Path.Combine(opts.TempPath, "verification"));
                    stackelVerifier.Arguments.Add("--stackelberg", "Dependencies/stackelberg-planner/src/fast-downward.py");
                    stackelVerifier.Run();
                    if (Failed)
                        return;

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

            //ConsoleHelper.WriteLineColor($"A total of {opts.TrainProblems.Count()} problems to train on.", ConsoleColor.Gray);
            //int problemCounter = 1;
            //int totalValidMetaActions = 0;
            //foreach (var problem in opts.TrainProblems)
            //{
            //    ConsoleHelper.WriteLineColor($"Training on problem '{new FileInfo(problem).Name}' started [{problemCounter++} of {opts.TrainProblems.Count()}]", ConsoleColor.Gray);
            //    // Purge temp folder
            //    new DirectoryInfo(opts.TempPath).Delete(true);

            //    var rootedProblem = PathHelper.RootPath(problem);

            //    // Generate Meta Actions
            //    ConsoleHelper.WriteLineColor($"Generating meta actions", ConsoleColor.Gray);
            //    ArgsCaller metaCaller = GetDotnetRunner("MetaActionGenerator");
            //    metaCaller.Arguments.Add("--domain", opts.DomainPath);
            //    metaCaller.Arguments.Add("--macros", Path.Combine(opts.TempPath, "macros"));
            //    metaCaller.Arguments.Add("--output", Path.Combine(opts.TempPath, "metaActions"));
            //    metaCaller.Run();
            //    if (Failed)
            //        return;
            //    var allMetaActions = new DirectoryInfo(Path.Combine(opts.TempPath, "metaActions")).GetFiles();
            //    ConsoleHelper.WriteLineColor($"A total of {allMetaActions.Length} meta actions found.", ConsoleColor.Gray);

            //    int counter = 1;
            //    foreach (var metaAction in allMetaActions)
            //    {
            //        ConsoleHelper.WriteLineColor($"Testing meta action {counter++} out of {allMetaActions.Length}.", ConsoleColor.Gray);
            //        // Compile Meta Actions
            //        ConsoleHelper.WriteLineColor($"Compiling meta action.", ConsoleColor.Gray);
            //        ArgsCaller stackelCompiler = GetDotnetRunner("StacklebergCompiler");
            //        stackelCompiler.Arguments.Add("--domain", opts.DomainPath);
            //        stackelCompiler.Arguments.Add("--problem", rootedProblem);
            //        stackelCompiler.Arguments.Add("--meta-action", metaAction.FullName);
            //        stackelCompiler.Arguments.Add("--output", Path.Combine(opts.TempPath, "compiled"));
            //        stackelCompiler.Run();
            //        if (Failed)
            //            return;

            //        // Verify Meta Actions
            //        ConsoleHelper.WriteLineColor($"Verifying meta action.", ConsoleColor.Gray);
            //        ArgsCaller stackelVerifier = GetDotnetRunner("StackelbergVerifier");
            //        stackelVerifier.StdOut -= PrintStdOut;
            //        stackelVerifier.StdOut += PrintStdOutVerifier;
            //        stackelVerifier.Arguments.Add("--domain", Path.Combine(opts.TempPath, "compiled", "simplified_domain.pddl"));
            //        stackelVerifier.Arguments.Add("--problem", Path.Combine(opts.TempPath, "compiled", "simplified_problem.pddl"));
            //        stackelVerifier.Arguments.Add("--output", Path.Combine(opts.TempPath, "verification"));
            //        stackelVerifier.Arguments.Add("--stackelberg", "Dependencies/stackelberg-planner/src/fast-downward.py");
            //        stackelVerifier.Run();
            //        if (Failed)
            //            return;

            //        // Output Valid Meta Actions
            //        if (!Directory.Exists(Path.Combine(opts.OutputPath, "valid")))
            //            Directory.CreateDirectory(Path.Combine(opts.OutputPath, "valid"));
            //        if (isValid)
            //        {
            //            ConsoleHelper.WriteLineColor($"Meta action was valid.", ConsoleColor.Green);
            //            totalValidMetaActions++;
            //            File.WriteAllText(Path.Combine(opts.OutputPath, "valid", $"meta{counter}.pddl"), File.ReadAllText(metaAction.FullName));
            //        }
            //        else
            //        {
            //            ConsoleHelper.WriteLineColor($"Meta action was invalid.", ConsoleColor.Red);
            //        }
            //        isValid = false;
            //    }
            //}
            //ConsoleHelper.WriteLineColor($"A total of {totalValidMetaActions} valid meta actions was found.", ConsoleColor.Green);
        }

        private static List<FileInfo> GenerateMacros(string domain, string problems, string tempPath)
        {
            var macroGenerator = GetRustRunner("macros");
            macroGenerator.Arguments.Add("-d", domain);
            macroGenerator.Arguments.Add("-p", problems);
            macroGenerator.Arguments.Add("-o", Path.Combine(tempPath, "macros"));
            macroGenerator.Arguments.Add("-c", "Dependencies/CSMs/src");
            macroGenerator.Arguments.Add("-f", "Dependencies/fast-downward/fast-downward.py");
            macroGenerator.Run();
            return new DirectoryInfo(Path.Combine(tempPath, "macros")).GetFiles().ToList();
        }

        private static List<FileInfo> GenerateMetaActions(string domain, string tempPath)
        {
            ArgsCaller metaCaller = GetDotnetRunner("MetaActionGenerator");
            metaCaller.Arguments.Add("--domain", domain);
            metaCaller.Arguments.Add("--macros", Path.Combine(tempPath, "macros"));
            metaCaller.Arguments.Add("--output", Path.Combine(tempPath, "metaActions"));
            metaCaller.Run();
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
            runner.Arguments.Add("--manifest-path", $"{project}/Cargo.toml");
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