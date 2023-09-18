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

            var depot = new Benchmark(benchmarkPath);

            ISampler sampler = new RandomSampler(1);
            var subset = sampler.Sample(depot.ProblemPaths, 2);

            IPlanFetcher fetcher = new FastDownwardPlanFetcher(
                Path.Join(projectPath, "PlanSamples", "depot"),
                "python",
                Path.Join(projectPath, "Dependencies", "fast-downward", "fast-downward.py"),
                "--alias lama-first",
                true
                );

            fetcher.Fetch(depot.DomainPath, subset);
        }
    }
}
