using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Tools.Benchmarks
{
    public class Benchmark
    {
        public string Name { get; set; }
        public string DomainPath { get; set; }
        public List<string> ProblemPaths { get; set; }

        [JsonConstructor]
        public Benchmark(string name, string domainPath, List<string> problemPaths)
        {
            Name = name;
            DomainPath = domainPath;
            ProblemPaths = problemPaths;
        }

        public Benchmark(string benchmarkFile) 
        {
            if (!File.Exists(benchmarkFile))
                throw new IOException($"Benchmark file not found: {benchmarkFile}");

            var parsed = JsonSerializer.Deserialize<Benchmark>(File.ReadAllText(benchmarkFile), new JsonSerializerOptions() { PropertyNameCaseInsensitive = false });
            if (parsed == null)
                throw new IOException($"Benchmark file invalid: {benchmarkFile}");

            Name = parsed.Name;
            DomainPath = parsed.DomainPath;
            ProblemPaths = parsed.ProblemPaths;
        }
    }
}
