using CommandLine;
using System;
using System.Diagnostics;
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

            ConsoleHelper.WriteLineColor($"A total of {opts.TrainProblems.Count()} problems to train on.", ConsoleColor.Gray);
            int problemCounter = 1;
            int totalValidMetaActions = 0;
            foreach (var problem in opts.TrainProblems)
            {
                ConsoleHelper.WriteLineColor($"Training on problem '{new FileInfo(problem).Name}' started [{problemCounter++} of {opts.TrainProblems.Count()}]", ConsoleColor.Gray);
                // Purge temp folder
                new DirectoryInfo(opts.TempPath).Delete(true);

                var rootedProblem = PathHelper.RootPath(problem);

                // Generate Macros
                ConsoleHelper.WriteLineColor($"Generating macros", ConsoleColor.Gray);
                Directory.CreateDirectory(Path.Combine(opts.TempPath, "macros"));
                File.WriteAllText(Path.Combine(opts.TempPath, "macros", "macro1.pddl"), "(:action pick_mcr_move_mcr_drop\r\n:parameters ( ?r - robot ?obj - object ?room - room ?g - gripper ?tox4 - room)\r\n:precondition (and (at ?obj ?room)(at-robby ?r ?room)(free ?r ?g)(stai_at ?obj ?room)(stai_free ?r ?g)(stag_at ?obj ?tox4))\r\n:effect (and (at-robby ?r ?tox4)(at ?obj ?tox4)(free ?r ?g)(not (at ?obj ?room))(not (at-robby ?r ?room))(not (carry ?r ?obj ?g)))\r\n)");

                var allMacros = new DirectoryInfo(Path.Combine(opts.TempPath, "macros")).GetFiles();
                ConsoleHelper.WriteLineColor($"A total of {allMacros.Length} macros found.", ConsoleColor.Gray);

                // Generate Meta Actions
                ConsoleHelper.WriteLineColor($"Generating meta actions", ConsoleColor.Gray);
                ArgsCaller metaCaller = GetDotnetRunner("MetaActionGenerator");
                metaCaller.Arguments.Add("--domain", opts.DomainPath);
                metaCaller.Arguments.Add("--macros", Path.Combine(opts.TempPath, "macros"));
                metaCaller.Arguments.Add("--output", Path.Combine(opts.TempPath, "metaActions"));
                metaCaller.Run();
                if (Failed)
                    return;
                var allMetaActions = new DirectoryInfo(Path.Combine(opts.TempPath, "metaActions")).GetFiles();
                ConsoleHelper.WriteLineColor($"A total of {allMetaActions.Length} meta actions found.", ConsoleColor.Gray);

                int counter = 1;
                foreach (var metaAction in allMetaActions)
                {
                    ConsoleHelper.WriteLineColor($"Testing meta action {counter++} out of {allMetaActions.Length}.", ConsoleColor.Gray);
                    // Compile Meta Actions
                    ConsoleHelper.WriteLineColor($"Compiling meta action.", ConsoleColor.Gray);
                    ArgsCaller stackelCompiler = GetDotnetRunner("StacklebergCompiler");
                    stackelCompiler.Arguments.Add("--domain", opts.DomainPath);
                    stackelCompiler.Arguments.Add("--problem", rootedProblem);
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
                    if (!Directory.Exists(Path.Combine(opts.OutputPath, "valid")))
                        Directory.CreateDirectory(Path.Combine(opts.OutputPath, "valid"));
                    if (isValid)
                    {
                        ConsoleHelper.WriteLineColor($"Meta action was valid.", ConsoleColor.Green);
                        totalValidMetaActions++;
                        File.WriteAllText(Path.Combine(opts.OutputPath, "valid", $"meta{counter}.pddl"), File.ReadAllText(metaAction.FullName));
                    }
                    else
                    {
                        ConsoleHelper.WriteLineColor($"Meta action was invalid.", ConsoleColor.Red);
                    }
                    isValid = false;
                }
            }
            ConsoleHelper.WriteLineColor($"A total of {totalValidMetaActions} valid meta actions was found.", ConsoleColor.Green);
        }

        private static ArgsCaller GetDotnetRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller("dotnet");
            isValid = false;
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("run", "");
            runner.Arguments.Add("--no-restore", "");
            runner.Arguments.Add("--no-build", "");
            runner.Arguments.Add("--project", $"{project} --");
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