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
    public class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<PlanSampleGeneratorOptions>(args)
              .WithParsed(RunPlanSampleGeneration)
              .WithNotParsed(HandleParseError);
        }

        public static void RunPlanSampleGeneration(PlanSampleGeneratorOptions opts)
        {
            var benchmark = ParseBenchmarkFile(opts.BennchmarkPath);

            GeneratePlanSamples(benchmark, opts);
        }

        public static Benchmark ParseBenchmarkFile(string path)
        {
            ConsoleHelper.WriteLineColor("Parsing benchmark file...", ConsoleColor.DarkGray);
            if (!File.Exists(path))
                throw new FileNotFoundException("The given benchmark file was not found!");
            var benchmarkFile = new Benchmark(path);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return benchmarkFile;
        }

        private static void GeneratePlanSamples(Benchmark benchmark, PlanSampleGeneratorOptions opts)
        {
            ConsoleHelper.WriteLineColor("Generating Plan Samples...", ConsoleColor.DarkGray);

            IPlanFetcher fetcher = new FastDownwardPlanFetcher(
                opts.OutPath,
                opts.DataPath,
                opts.PythonPrefix,
                opts.FastDownwardPath,
                opts.FastDownwardSearch);
            fetcher.Fetch(benchmark, opts.Samples, opts.Multithread, opts.Seed);

            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}
