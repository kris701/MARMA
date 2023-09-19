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
        [Option('d', "deps", Required = true, HelpText = "Path to the dependency file to use")]
        public string DependencyPath { get; set; } = "";
    }
}
