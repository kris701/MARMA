using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroExtractor
{
    public class Options
    {
        [Option("leader-plans", Required = true, HelpText = "Path the the leader plan files to extract macros from.")]
        public IEnumerable<string> LeaderPlans { get; set; } = new List<string>();
        [Option("follower-plans", Required = true, HelpText = "Path the the follower plan files to extract macros from.")]
        public IEnumerable<string> FollowerPlans { get; set; } = new List<string>();
        [Option("output", Required = false, HelpText = "Where to output macros.", Default = "output/train/replacementMacros")]
        public string OutputPath { get; set; } = "output/train/replacementMacros";
    }
}
