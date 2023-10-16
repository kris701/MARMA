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
            opts.MetaPath = PathHelper.RootPath(opts.MetaPath);

            if (!Directory.Exists(opts.TempPath))
                Directory.CreateDirectory(opts.TempPath);
            if (!Directory.Exists(opts.MetaPath))
                Directory.CreateDirectory(opts.MetaPath);

            foreach (var problem in opts.TrainProblems)
            {
                foreach (var file in new DirectoryInfo(opts.TempPath).GetFiles())
                    file.Delete();

                var rootedProblem = PathHelper.RootPath(problem);

                // Generate Macros


                // Generate Meta Actions
                ArgsCaller metaCaller = new ArgsCaller("MetaActionGenerator/bin/Debug/net7.0/MetaActionGenerator.exe");
                metaCaller.StdOut += PrintStdOut;
                metaCaller.StdErr += PrintStdErr;
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
                    ArgsCaller stackelCompiler = new ArgsCaller("StacklebergCompiler/bin/Debug/net7.0/StacklebergCompiler.exe");
                    metaCaller.StdOut += PrintStdOut;
                    metaCaller.StdErr += PrintStdErr;
                    metaCaller.Arguments.Add("--domain", opts.DomainPath);
                    metaCaller.Arguments.Add("--problem", rootedProblem);
                    metaCaller.Arguments.Add("--meta-action", metaAction.FullName);
                    metaCaller.Arguments.Add("--output", Path.Combine(opts.TempPath, "compiled"));
                    metaCaller.Run();
                    if (Failed)
                        return;

                    // Verify Meta Actions
                    ArgsCaller stackelVerifier = new ArgsCaller("StackelbergVerifier/bin/Debug/net7.0/StackelbergVerifier.exe");
                    isValid = false;
                    stackelVerifier.StdOut += PrintStdOutVerifier;
                    stackelVerifier.StdErr += PrintStdErr;
                    stackelVerifier.Arguments.Add("--domain", opts.DomainPath);
                    stackelVerifier.Arguments.Add("--problem", rootedProblem);
                    stackelVerifier.Arguments.Add("--output", Path.Combine(opts.TempPath, "verification"));
                    stackelVerifier.Arguments.Add("--stackelberg", "Dependencies/stackelberg-sls/src/fast-downward.py");
                    stackelVerifier.Run();
                    if (Failed)
                        return;

                    // Output Valid Meta Actions
                    if (isValid)
                        File.WriteAllText(Path.Combine(opts.MetaPath, "valid", $"meta{counter++}.pddl"), File.ReadAllText(metaAction.FullName));
                    isValid = false;
                }
            }
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