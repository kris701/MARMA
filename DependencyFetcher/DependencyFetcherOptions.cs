using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyFetcher
{
    public class DependencyFetcherOptions
    {
        [Option("root", Required = true, HelpText = "Path to the root to install dependencies to")]
        public string RootPath { get; set; } = "";
        [Option("deps", Required = true, HelpText = "Path to the dependency file to use")]
        public string DependencyPath { get; set; } = "";
    }
}
