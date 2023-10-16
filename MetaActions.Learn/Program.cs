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

            if (!Directory.Exists(opts.TempPath))
                Directory.CreateDirectory(opts.TempPath);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);

            foreach (var problem in opts.TrainProblems)
            {
                foreach (var file in new DirectoryInfo(opts.TempPath).GetFiles())
                    file.Delete();

                var rootedProblem = PathHelper.RootPath(problem);

                // Generate Macros
                Directory.CreateDirectory(Path.Combine(opts.TempPath, "macros"));
                File.WriteAllText(Path.Combine(opts.TempPath, "macros", "macro1.pddl"), "(:action pick_mcr_move_mcr_drop\r\n:parameters ( ?r - robot ?obj - object ?room - room ?g - gripper ?tox4 - room)\r\n:precondition (and (at ?obj ?room)(at-robby ?r ?room)(free ?r ?g)(stai_at ?obj ?room)(stai_free ?r ?g)(stag_at ?obj ?tox4))\r\n:effect (and (at-robby ?r ?tox4)(at ?obj ?tox4)(free ?r ?g)(not (at ?obj ?room))(not (at-robby ?r ?room))(not (carry ?r ?obj ?g)))\r\n)");

                // Generate Meta Actions
                ArgsCaller metaCaller = GetDotnetRunner("MetaActionGenerator");
                metaCaller.Arguments.Add("--domain", opts.DomainPath);
                metaCaller.Arguments.Add("--macros", Path.Combine(opts.TempPath, "macros"));
                metaCaller.Arguments.Add("--output", Path.Combine(opts.TempPath, "metaActions"));
                metaCaller.Run();
                if (Failed)
                    return;

                // Compile Meta Actions
                int counter = 0;
                foreach (var metaAction in new DirectoryInfo(Path.Combine(opts.TempPath, "metaActions")).GetFiles())
                {
                    ArgsCaller stackelCompiler = GetDotnetRunner("StacklebergCompiler");
                    stackelCompiler.Arguments.Add("--domain", opts.DomainPath);
                    stackelCompiler.Arguments.Add("--problem", rootedProblem);
                    stackelCompiler.Arguments.Add("--meta-action", metaAction.FullName);
                    stackelCompiler.Arguments.Add("--output", Path.Combine(opts.TempPath, "compiled"));
                    stackelCompiler.Run();
                    if (Failed)
                        return;

                    // Verify Meta Actions
                    ArgsCaller stackelVerifier = GetDotnetRunner("StackelbergVerifier");
                    stackelVerifier.StdOut += PrintStdOutVerifier;
                    stackelVerifier.Arguments.Add("--domain", Path.Combine(opts.TempPath, "compiled", "simplified_domain.pddl"));
                    stackelVerifier.Arguments.Add("--problem", Path.Combine(opts.TempPath, "compiled", "simplified_problem.pddl"));
                    stackelVerifier.Arguments.Add("--output", Path.Combine(opts.TempPath, "verification"));
                    stackelVerifier.Arguments.Add("--stackelberg", "Dependencies/stackelberg-sls/src/fast-downward.py");
                    stackelVerifier.Run();
                    if (Failed)
                        return;

                    // Output Valid Meta Actions
                    if (isValid)
                        File.WriteAllText(Path.Combine(opts.OutputPath, "valid", $"meta{counter++}.pddl"), File.ReadAllText(metaAction.FullName));
                    isValid = false;
                }
            }
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
                if (e.Data.Contains(""))
                    isValid = true;
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Gray);
            }
        }

        private static void PrintStdOut(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Gray);
        }

        private static bool Failed = false;
        private static void PrintStdErr(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Failed = true;
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Red);
            }
        }
    }
}