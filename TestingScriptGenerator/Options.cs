using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingScriptGenerator
{
    public class Options
    {
        [Option("domains", Required = true, HelpText = "Path to the domain files to use. You can use wildcards (*) for full directory names, and partial file names.")]
        public IEnumerable<string> Domains { get; set; } = new List<string>();
        [Option("train-problems", Required = true, HelpText = "Problems to train from. You can use wildcards (*) for full directory names, and partial file names.")]
        public IEnumerable<string> TrainProblems { get; set; } = new List<string>();
        [Option("test-problems", Required = true, HelpText = "Problems to test on later. You can use wildcards (*) for full directory names, and partial file names.")]
        public IEnumerable<string> TestProblems { get; set; } = new List<string>();
        [Option("split", Required = true, HelpText = "How many problems should be tested on")]
        public int TrainSplit { get; set; } = 10;
    }
}
