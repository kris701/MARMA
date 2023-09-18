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
        public string FastDownwardPath { get; set; }
        public string FastDownwardArguments { get; set; }

        public PlanSampleGenerator(string domainPath, List<string> problemPaths, int sampleCount, string outputPath, string fastDownwardPath, string fastDownwardArguments)
        {
            DomainPath = domainPath;
            ProblemPaths = problemPaths;
            SampleCount = sampleCount;
            OutputPath = outputPath;
            FastDownwardPath = fastDownwardPath;
            FastDownwardArguments = fastDownwardArguments;
        }

        public void Sample(int seed = -1)
        {
            Random rnd = GetRandomizer(seed);
            var subset = GetRandomSubset(ProblemPaths, rnd, SampleCount);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = $"clone https://github.com/aibasel/downward-benchmarks \"{path}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                }
            };
            process.OutputDataReceived += (sender, e) => {
                Console.WriteLine(e.Data);
            };
            process.ErrorDataReceived += (sender, e) => {
                Console.WriteLine(e.Data);
            };
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
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
