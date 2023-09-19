using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using Tools.Benchmarks;

namespace PlanSampleGenerator
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<PlanSampleGeneratorOptions>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void RunOptions(PlanSampleGeneratorOptions opts)
        {
            var benchmark = ParseBenchmarkFile(opts.BennchmarkPath);

            GeneratePlanSamples(benchmark, opts);
        }

        private static void GeneratePlanSamples(Benchmark benchmark, PlanSampleGeneratorOptions opts)
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
