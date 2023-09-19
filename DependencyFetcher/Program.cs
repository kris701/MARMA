using CommandLine;
using System;
using Tools;

namespace DependencyFetcher
{
    public class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<DependencyFetcherOptions>(args)
              .WithParsed(RunDependencyChecker)
              .WithNotParsed(HandleParseError);
        }

        public static void RunDependencyChecker(DependencyFetcherOptions opts)
        {
            if (!File.Exists(opts.DependencyPath))
                throw new IOException($"File not found: {opts.DependencyPath}");
            if (!Directory.Exists(opts.RootPath))
                throw new IOException($"Root not found: {opts.RootPath}");

            CheckDependencies(opts.DependencyPath, opts.RootPath);
        }

        private static void CheckDependencies(string path, string root)
        {
            ConsoleHelper.WriteLineColor("Checking Dependencies...", ConsoleColor.DarkGray);
            IDependencyChecker checker = new DependencyChecker(path);
            checker.CheckDependencies(root);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}