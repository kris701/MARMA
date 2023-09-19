using CommandLine;
using CommandLine.Text;
using PlanSampleGenerator;
using System;
using Tools;
using Tools.Benchmarks;

namespace Toolchain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
                ConsoleHelper.WriteLineColor($"{error}", ConsoleColor.Red);
        }

        static void RunOptions(Options opts)
        {
            if (!File.Exists(opts.BennchmarkPath))
                throw new FileNotFoundException("The given benchmark file was not found!");

            ConsoleHelper.WriteLineColor("Toolchain have started...", ConsoleColor.DarkGray);

            var projectPath = ProjectHelper.GetProjectPath();

            if (opts.CheckDependencies)
                CheckDependencies();

            var benchmark = ParseBenchmarkFile(opts.BennchmarkPath);

            GeneratePlanSamples(benchmark, opts.Samples, opts.Seed, opts.Multithread, projectPath);

            ConsoleHelper.WriteLineColor("Toolchain have finished!", ConsoleColor.DarkGray);

        }

        private static void CheckDependencies()
        {
            ConsoleHelper.WriteLineColor("Checking Dependencies...", ConsoleColor.DarkGray);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }

        private static Benchmark ParseBenchmarkFile(string path)
        {
            ConsoleHelper.WriteLineColor("Parsing benchmark file...", ConsoleColor.DarkGray);
            var benchmarkFile = new Benchmark(path);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return benchmarkFile;
        }

        private static void GeneratePlanSamples(Benchmark benchmark, int samples, int seed, bool multithread, string projectPath)
        {
            ConsoleHelper.WriteLineColor("Generating Plan Samples...", ConsoleColor.DarkGray);

            IPlanFetcher fetcher = new FastDownwardPlanFetcher(
                "python",
                Path.Combine(projectPath, "Dependencies", "fast-downward", "fast-downward.py"),
                "--alias lama-first");
            fetcher.Fetch(benchmark, samples, multithread, seed);

            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}