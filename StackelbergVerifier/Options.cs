using CommandLine;

namespace StackelbergVerifier
{
    public class Options
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use")]
        public string DomainFilePath { get; set; } = "";
        [Option("problem", Required = true, HelpText = "Path to the problem file to use")]
        public string ProblemFilePath { get; set; } = "";
        [Option("output", Required = false, HelpText = "Path to output files to")]
        public string OutputPath { get; set; } = "";
        [Option("iseasy", Required = false, HelpText = "Is the problem an easy one", Default = false)]
        public bool IsEasyProblem { get; set; } = false;
        [Option("reachability-check", Required = false, HelpText = "Perform reachability check before stackelberg planning?", Default = false)]
        public bool ReachabilityCheck { get; set; } = false;
    }
}
