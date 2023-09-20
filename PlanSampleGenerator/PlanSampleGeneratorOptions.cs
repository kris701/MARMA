using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanSampleGenerator
{
    public class PlanSampleGeneratorOptions
    {
        // General
        [Option("benchmark", Required = true, HelpText = "Path to the benchmark file to use")]
        public string BennchmarkPath { get; set; } = "";
        [Option("outdir", Required = true, HelpText = "Path to put the output in")]
        public string OutPath { get; set; } = "";
        [Option("datadir", Required = true, HelpText = "Path to where the benchmark data is")]
        public string DataPath { get; set; } = "";

        // Plan Sample Generation
        [Option("samples", Required = true, HelpText = "How many samples are to be takes as the 'train' set")]
        public int Samples { get; set; } = 0;
        [Option("py", Required = false, HelpText = "What python prefix to use", Default = "python")]
        public string PythonPrefix { get; set; } = "python";
        [Option("fastdownward", Required = false, HelpText = "Path to fast-downward", Default = "fast-downward.py")]
        public string FastDownwardPath { get; set; } = "fast-downward.py";
        [Option("alias", Required = false, HelpText = "Alias (or search) to use with fast downward", Default = "--alias lama-first")]
        public string FastDownwardSearch { get; set; } = "--alias lama-first";

        // Misc
        [Option("seed", Required = false, HelpText = "Optional seed for all the random number generation", Default = -1)]
        public int Seed { get; set; } = -1;
        [Option("multithread", Required = false, HelpText = "If parts of the toolchain should run multithreaded or not.")]
        public bool Multithread { get; set; } = false;
    }
}
