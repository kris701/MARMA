using CommandLine;

namespace MetaActions.Test
{
    public class Options
    {
        [Option("data", Required = true, HelpText = "Zip file containing all the training data.")]
        public string DataFile { get; set; } = "";
        [Option("tempDir", Required = false, HelpText = "Path where all the intermediate files can be saved.", Default = "temp/test")]
        public string TempPath { get; set; } = "temp/test";
        [Option("output", Required = false, HelpText = "Path where all the output will be saved", Default = "output/test")]
        public string OutputPath { get; set; } = "output/test";
        [Option("rebuild", Required = false, HelpText = "Rebuild toolchain?", Default = false)]
        public bool Rebuild { get; set; } = false;
    }
}
