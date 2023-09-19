using CommandLine;
using CommandLine.Text;
using DependencyFetcher;
using PlanSampleGenerator;
using System;
using Tools;
using Tools.Benchmarks;

namespace Toolchain
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ToolchainOptions>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void RunOptions(ToolchainOptions opts)
        {
            ConsoleHelper.WriteLineColor("Toolchain have started...", ConsoleColor.DarkGray);

            if (opts.DependencyPath != "")
                CheckDependencies(opts.DependencyPath);

            var benchmark = ParseBenchmarkFile(opts.BennchmarkPath);

            GeneratePlanSamples(benchmark, opts);

            ConsoleHelper.WriteLineColor("Toolchain have finished!", ConsoleColor.DarkGray);
        }

        private static void CheckDependencies(string path)
        {
            ConsoleHelper.WriteLineColor("Checking Dependencies...", ConsoleColor.DarkGray);
            IDependencyChecker checker = new DependencyChecker(path);
            checker.CheckDependencies();
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }

        private static Benchmark ParseBenchmarkFile(string path)
        {
            ConsoleHelper.WriteLineColor("Parsing benchmark file...", ConsoleColor.DarkGray);
            if (!File.Exists(path))
                throw new FileNotFoundException("The given benchmark file was not found!");
            var benchmarkFile = new Benchmark(path);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return benchmarkFile;
        }

        private static void GeneratePlanSamples(Benchmark benchmark, ToolchainOptions opts)
        {
            ConsoleHelper.WriteLineColor("Generating Plan Samples...", ConsoleColor.DarkGray);

            IPlanFetcher fetcher = new FastDownwardPlanFetcher(
                opts.PythonPrefix,
                opts.FastDownwardPath,
                opts.FastDownwardSearch);
            fetcher.Fetch(benchmark, opts.Samples, opts.Multithread, opts.Seed);

            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}