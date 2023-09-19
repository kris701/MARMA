using CommandLine.Text;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolchain
{
    public class ToolchainOptions
    {
        [Option('b', "benchmark", Required = true, HelpText = "Path to the benchmark file to use")]
        public string BennchmarkPath { get; set; } = "";
        [Option('s', "samples", Required = true, HelpText = "How many samples are to be takes as the 'train' set")]
        public int Samples { get; set; } = 0;

        [Option('d', "deps", Required = false, HelpText = "Check if required dependencies are installed")]
        public bool CheckDependencies { get; set; } = false;
        [Option('e', "seed", Required = false, HelpText = "Optional seed for all the random number generation")]
        public int Seed { get; set; } = -1;
        [Option('m', "multithread", Required = false, HelpText = "If parts of the toolchain should run multithreaded or not.")]
        public bool Multithread { get; set; } = false;

        [Usage(ApplicationAlias = "Toolchain")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                        new Example("Run the toolchain on a benchmark file", new ToolchainOptions { BennchmarkPath = "depot.json", Samples = 2 })
                      };
            }
        }
    }
}
