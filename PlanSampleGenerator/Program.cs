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

            IPlanFetcher fetcher = new FastDownwardPlanFetcher(
                "python",
                Path.Join(projectPath, "Dependencies", "fast-downward", "fast-downward.py"),
                "--alias lama-first"
                );

            fetcher.Fetch(depot, 2, true, 1);
        }
    }
}
