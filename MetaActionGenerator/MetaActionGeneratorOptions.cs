using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActionGenerator
{
    public class MetaActionGeneratorOptions
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use")]
        public string DomainFilePath { get; set; } = "";
        [Option("macros", Required = true, HelpText = "Path to the folder containing macro actions.")]
        public string MacroActionPath { get; set; } = "";
        [Option("output", Required = true, HelpText = "Where to output the generated meta actions")]
        public string OutputPath { get; set; } = "";
    }
}
