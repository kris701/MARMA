using System;
using Tools;
using Tools.Benchmarks;

namespace PlanSampleGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Depot time!");

            var projectPath = ProjectHelper.GetProjectPath();
            var benchmarkPath = Path.Join(projectPath, "Benchmarks", "depot.json");

            var barman = new Benchmark(benchmarkPath);

            IPlanSampleGenerator generator = new PlanSampleGenerator(
                barman.DomainPath,
                barman.ProblemPaths,
                2,
                Path.Join(projectPath, "PlanSamples", "depot"),
                "python",
                Path.Join(projectPath, "Dependencies", "fast-downward", "fast-downward.py"),
                "--alias lama-first"
                );

            generator.Sample(-1);
        }
    }
}