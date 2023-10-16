using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Test
{
    public class Options
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use.")]
        public string DomainPath { get; set; } = "";
        [Option("problems", Required = true, HelpText = "Problems to test on.")]
        public IEnumerable<string> TestingProblems { get; set; }
        [Option("meta-actions", Required = true, HelpText = "Folder containing valid meta actions.")]
        public string MetaActionsPath { get; set; } = "";
        [Option("tempDir", Required = false, HelpText = "Path where all the intermediate files can be saved.", Default = "temp/test")]
        public string TempPath { get; set; } = "temp/test";
        [Option("output", Required = false, HelpText = "Path where all the output will be saved", Default = "output/test")]
        public string OutputPath { get; set; } = "output/test";
    }
}
