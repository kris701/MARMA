using CommandLine;
using System;
using Tools;

namespace DependencyFetcher
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<DependencyFetcherOptions>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void RunOptions(DependencyFetcherOptions opts)
        {
            if (!File.Exists(opts.DependencyPath))
                throw new IOException($"File not found: {opts.DependencyPath}");

            CheckDependencies(opts.DependencyPath);
        }

        private static void CheckDependencies(string path)
        {
            ConsoleHelper.WriteLineColor("Checking Dependencies...", ConsoleColor.DarkGray);
            IDependencyChecker checker = new DependencyChecker(path);
            checker.CheckDependencies();
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}