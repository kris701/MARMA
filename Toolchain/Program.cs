using CommandLine;
using CommandLine.Text;
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

            var projectPath = ProjectHelper.GetProjectPath();

            if (opts.CheckDependencies)
                CheckDependencies();

            var benchmark = ParseBenchmarkFile(opts.BennchmarkPath);

            GeneratePlanSamples(benchmark, opts);

            ConsoleHelper.WriteLineColor("Toolchain have finished!", ConsoleColor.DarkGray);
        }

        private static void CheckDependencies()
        {
            ConsoleHelper.WriteLineColor("Checking Dependencies...", ConsoleColor.DarkGray);
            var dependenciesFile = Path.Combine(projectFolder, "Dependencies", "dependencies.json");
            IDependencyChecker checker = new DependencyChecker(dependenciesFile);
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