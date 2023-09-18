using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanSampleGenerator
{
    public class PlanSampleGenerator : IPlanSampleGenerator
    {
        public string DomainPath { get; set; }
        public List<string> ProblemPaths { get; set; }
        public int SampleCount { get; set; }
        public string OutputPath { get; set; }
        public string PythonPrefix { get; set; }
        public string FastDownwardPath { get; set; }
        public string FastDownwardSearch { get; set; }

        public PlanSampleGenerator(string domainPath, List<string> problemPaths, int sampleCount, string outputPath, string pythonPrefix, string fastDownwardPath, string fastDownwardSearch)
        {
            DomainPath = domainPath;
            ProblemPaths = problemPaths;
            SampleCount = sampleCount;
            OutputPath = outputPath;
            PythonPrefix = pythonPrefix;
            FastDownwardPath = fastDownwardPath;
            FastDownwardSearch = fastDownwardSearch;
        }

        public void Sample(int seed = -1)
        {
            Random rnd = GetRandomizer(seed);
            var subset = GetRandomSubset(ProblemPaths, rnd, SampleCount);

            if (Directory.Exists(OutputPath))
                Directory.Delete(OutputPath);
            Directory.CreateDirectory(OutputPath);

            List<Task> tasks = new List<Task>();
            int id = 0;
            foreach (var sub in subset)
                tasks.Add(SampleDomainProblemCombinationAsync(DomainPath, sub, id++));
            foreach (var task in tasks)
                task.Start();
            Task.WhenAll(tasks);
        }

        private async Task SampleDomainProblemCombinationAsync(string domain, string problem, int id)
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append($"{FastDownwardPath} ");
            string fileName = $"{new FileInfo(domain).Name}-{id}";
            sb.Append($"--plan-file '{Path.Combine(OutputPath, fileName)}' ");
            sb.Append($"'{domain}' ");
            sb.Append($"'{problem}' ");
            sb.Append($"--search \"{FastDownwardSearch}\"");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = PythonPrefix,
                    Arguments = sb.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
            process.ErrorDataReceived += (sender, e) => {
                Console.WriteLine(e.Data);
            };
            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }

        private List<string> GetRandomSubset(List<string> source, Random rnd, int count)
        {
            List<string> subset = new List<string>();

            while(subset.Count <= count)
            {
                var target = rnd.Next(0, source.Count);
                if (!subset.Contains(source[target]))
                    subset.Add(source[target]);
            }

            return subset;
        }

        private Random GetRandomizer(int seed)
        {
            if (seed == -1)
                return new Random();
            else
                return new Random(seed);
        }
    }
}
