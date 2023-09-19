using System.Diagnostics;
using System.Text;
using Tools;
using Tools.Benchmarks;

namespace PlanSampleGenerator
{
    public class FastDownwardPlanFetcher : IPlanFetcher
    {
        public string OutputPath { get; set; }
        public string DataPath { get; set; }
        public string PythonPrefix { get; set; }
        public string FastDownwardPath { get; set; }
        // Can either be a "--search" or "--alias"
        public string FastDownwardSearch { get; set; }

        public FastDownwardPlanFetcher(string outputPath, string dataPath, string pythonPrefix, string fastDownwardPath, string fastDownwardSearch)
        {
            if (!Path.IsPathRooted(outputPath))
                outputPath = Path.Join(Directory.GetCurrentDirectory(), outputPath);
            OutputPath = outputPath;
            if (!Path.IsPathRooted(dataPath))
                dataPath = Path.Join(Directory.GetCurrentDirectory(), dataPath);
            DataPath = dataPath;
            PythonPrefix = pythonPrefix;

            if (!Path.IsPathRooted(fastDownwardPath))
                fastDownwardPath = Path.Join(Directory.GetCurrentDirectory(), fastDownwardPath);
            FastDownwardPath = fastDownwardPath;
            FastDownwardSearch = fastDownwardSearch;
        }

        public void Fetch(Benchmark benchmark, int count, bool multithreaded = true, int seed = -1)
        {
            CreateFolders(benchmark.Name);

            var subset = benchmark.ProblemPaths.OrderBy(x => GetRandomizer(seed).Next()).Take(count).ToList();
            var remaining = benchmark.ProblemPaths.Where(x => !subset.Contains(x)).ToList();

            FetchAll(benchmark.Name, benchmark.DomainPath, subset, multithreaded);

            GenerateLogFiles(benchmark.Name, benchmark.DomainPath, subset, remaining);
        }

        private Random GetRandomizer(int seed)
        {
            if (seed == -1)
                return new Random();
            else
                return new Random(seed);
        }

        private void CreateFolders(string name)
        {
            if (Directory.Exists(Path.Combine(OutputPath, name)))
                Directory.Delete(Path.Combine(OutputPath, name), true);
            Directory.CreateDirectory(Path.Combine(OutputPath, name));
            Directory.CreateDirectory(Path.Combine(OutputPath, name, "Plans"));
        }

        private void FetchAll(string name, string domain, List<string> problems, bool multithreaded)
        {
            List<Task> tasks = new List<Task>();
            foreach (var problem in problems)
                tasks.Add(SampleDomainProblemCombinationAsync(name, domain, problem));
            foreach (var task in tasks)
            {
                task.Start();
                if (!multithreaded)
                    task.Wait();
            }
            Task.WaitAll(tasks.ToArray());
        }

        private Task SampleDomainProblemCombinationAsync(string name, string domain, string problem)
        {
            return new Task(() =>
            {
                StringBuilder sb = new StringBuilder("");
                sb.Append($"{FastDownwardPath} ");
                string fileName = $"{new FileInfo(domain).Name.Replace(".pddl", "")}-{new FileInfo(problem).Name.Replace(".pddl", "")}";
                sb.Append($"--plan-file \"{Path.Combine(OutputPath, name, "Plans", fileName)}.plan\" ");
                sb.Append($"--sas-file \"{Path.Combine(OutputPath, name, "Plans", fileName)}.sas\" ");

                if (FastDownwardSearch.StartsWith("--alias"))
                    sb.Append($"--alias \"lama-first\" ");
                sb.Append($"\"{Path.Combine(DataPath, domain)}\" ");
                sb.Append($"\"{Path.Combine(DataPath, problem)}\" ");
                if (FastDownwardSearch.StartsWith("--search"))
                    sb.Append($"--search \"{FastDownwardSearch}\"");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = PythonPrefix,
                        Arguments = sb.ToString(),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        //RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = OutputPath
                    }
                };
                //process.OutputDataReceived += (sender, e) =>
                //{
                //    Console.WriteLine(e.Data);
                //};
                process.ErrorDataReceived += (sender, e) =>
                {
                    Console.WriteLine(e.Data);
                };
                process.Start();
                process.BeginErrorReadLine();
                //process.BeginOutputReadLine();
                process.WaitForExit();
            });
        }

        private void GenerateLogFiles(string name, string domain, List<string> usedProblems, List<string> remainingProblems)
        {
            var usedBenchmark = new Benchmark(name, domain, usedProblems);
            usedBenchmark.Save(Path.Combine(OutputPath, name, "used.json"));
            var remainingBenchmark = new Benchmark(name, domain, remainingProblems);
            remainingBenchmark.Save(Path.Combine(OutputPath, name, "remaining.json"));
        }
    }
}
