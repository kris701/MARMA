using Tools.Benchmarks;

namespace PlanSampleGenerator
{
    public interface IPlanFetcher
    {
        public string OutputPath { get; set; }
        public string DataPath { get; set; }
        public string PythonPrefix { get; set; }
        public string FastDownwardPath { get; set; }
        // Can either be a "--search" or "--alias"
        public string FastDownwardSearch { get; set; }

        public void Fetch(Benchmark benchmark, int count, bool multithreaded = true, int seed = -1);
    }
}
