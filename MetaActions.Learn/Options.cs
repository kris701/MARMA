using CommandLine;

namespace MetaActions.Learn
{
    public class Options
    {
        [Option("domain", Required = true, HelpText = "Path to the domain file to use.")]
        public string DomainPath { get; set; } = "";
        [Option("problems", Required = true, HelpText = "Problems to train from.")]
        public IEnumerable<string> Problems { get; set; } = new List<string>();
        [Option("tempDir", Required = false, HelpText = "Path where all the intermediate files can be saved.", Default = "temp/train")]
        public string TempPath { get; set; } = "temp/train";
        [Option("output", Required = false, HelpText = "Path where all the output will be saved", Default = "output/train")]
        public string OutputPath { get; set; } = "output/train";
    }
}
