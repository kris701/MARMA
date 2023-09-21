using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanWalkerWrapper
{
    public class PlanWalkerWrapperOptions
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use")]
        public string DomainFilePath { get; set; } = "";
        [Option("problem", Required = true, HelpText = "Path to the problem file to use")]
        public string ProblemFilePath { get; set; } = "";
        [Option("meta-domain", Required = true, HelpText = "Path to meta domain")]
        public string MetaDomainPath { get; set; } = "";
        [Option("walker", Required = false, HelpText = "Path to the walker", Default = "plan-walker")]
        public string WalkerPath { get; set; } = "";

        [Option("fastdownward", Required = false, HelpText = "Path to fast-downward", Default = "fast-downward.py")]
        public string FastDownwardPath { get; set; } = "fast-downward.py";
        [Option("solution", Required = false, HelpText = "Path to solution file. If none given fast downward will generate one", Default = "")]
        public string SolutionPath { get; set; } = "";
        [Option("outplan", Required = false, HelpText = "Path the the output plan. If none given it prints to stdout", Default = "")]
        public string OutPlanPath { get; set; } = "";
    }
}
