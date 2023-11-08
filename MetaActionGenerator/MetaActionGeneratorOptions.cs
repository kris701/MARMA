using CommandLine;

namespace MetaActionGenerator
{
    public class MetaActionGeneratorOptions
    {
        [Option("macros", Required = true, HelpText = "Path to the folder containing macro actions.")]
        public string MacroActionPath { get; set; } = "";
        [Option("output", Required = true, HelpText = "Where to output the generated meta actions")]
        public string OutputPath { get; set; } = "";
    }
}
