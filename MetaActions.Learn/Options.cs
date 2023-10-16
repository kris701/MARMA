using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Learn
{
    public class Options
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use.")]
        public string DomainPath { get; set; } = "";
        [Option("problems", Required = true, HelpText = "Problems to train from.")]
        public IEnumerable<string> TrainProblems { get; set; }
        [Option("tempDir", Required = false, HelpText = "Path where all the intermediate files can be saved.", Default = "temp")]
        public string TempPath { get; set; } = "temp";
        [Option("output", Required = false, HelpText = "Path where all the output will be saved", Default = "output")]
        public string OutputPath { get; set; } = "output";
    }
}
