using CommandLine;

namespace MetaActions.Test
{
    public class Options
    {
        [Flags]
        public enum ReconstructionMethods
        {
            None = 0,
            FastDownward = 1
        }

        [Option("data", Required = true, HelpText = "Zip file containing all the training data.")]
        public string DataFile { get; set; } = "";
        [Option("tempDir", Required = false, HelpText = "Path where all the intermediate files can be saved.", Default = "temp/test")]
        public string TempPath { get; set; } = "temp/test";
        [Option("output", Required = false, HelpText = "Path where all the output will be saved", Default = "output/test")]
        public string OutputPath { get; set; } = "output/test";
        [Option("rebuild", Required = false, HelpText = "Rebuild toolchain?", Default = false)]
        public bool Rebuild { get; set; } = false;
        [Option("timelimit", Required = false, HelpText = "Time limit for each problem, in minutes", Default = 30)]
        public int TimeLimit { get; set; } = 30;
        [Option("alias", Required = false, HelpText = "Fast-Downward alias to use", Default = "lama-first")]
        public string Alias { get; set; } = "lama-first";
        [Option("multitask", Required = false, HelpText = "Run tests multitasked", Default = false)]
        public bool MultiTask { get; set; } = false;
        [Option("reconstructor", Required = false, HelpText = "What reconstruction method to use", Default = ReconstructionMethods.FastDownward)]
        public ReconstructionMethods ReconstructionMethod { get; set; } = ReconstructionMethods.FastDownward;
    }
}
