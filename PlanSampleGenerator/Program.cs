using CommandLine.Text;
using CommandLine;
using Tools;
using Tools.Benchmarks;

namespace PlanSampleGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<SampleGeneratorOptions>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
                ConsoleHelper.WriteLineColor($"{error}", ConsoleColor.Red);
        }

        static void RunOptions(SampleGeneratorOptions opts)
        {
            ConsoleHelper.WriteLineColor("Plan Sample Generator Started...", ConsoleColor.DarkGray);

            var benchmark = ParseBenchmarkFile(opts.BennchmarkPath);

            ConsoleHelper.WriteLineColor("Starting fetching...", ConsoleColor.DarkGray);
            IPlanFetcher fetcher = new FastDownwardPlanFetcher(
                opts.PythonPrefix,
                opts.FastDownwardPath,
                opts.FastDownwardSearch
                );
            fetcher.Fetch(benchmark, opts.Samples, opts.Multithread, opts.Seed);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Plan Sample Generator Done!", ConsoleColor.Green);
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
    }
}
