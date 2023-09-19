using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Tools.Benchmarks;

namespace Tools
{
    public abstract class BaseCLI
    {
        public static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
                ConsoleHelper.WriteLineColor($"{error}", ConsoleColor.Red);
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
    }
}
