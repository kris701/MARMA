using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackelbergVerifier
{
    public class StackelbergVerifierOptions
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use")]
        public string DomainFilePath { get; set; } = "";
        [Option("problem", Required = true, HelpText = "Path to the problem file to use")]
        public string ProblemFilePath { get; set; } = "";
        [Option("output", Required = false, HelpText = "Path to output files to")]
        public string OutputPath { get; set; } = "";
        [Option("py", Required = false, HelpText = "What python prefix to use", Default = "python2")]
        public string PythonPrefix { get; set; } = "python2";
        [Option("stackelberg", Required = false, HelpText = "Path to stackelberg planner", Default = "fast-downward.py")]
        public string StackelbergPath { get; set; } = "fast-downward.py";
        [Option("iseasy", Required = false, HelpText = "Is the problem an easy one", Default = true)]
        public bool IsEasyProblem { get; set; } = true;
    }
}
