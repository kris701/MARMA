﻿using CommandLine;

namespace MetaActions.Learn
{
    public class Options
    {
        [Option("domains", Required = true, HelpText = "Path to the domain files to use.")]
        public IEnumerable<string> Domains { get; set; } = new List<string>();
        [Option("train-problems", Required = true, HelpText = "Problems to train from.")]
        public IEnumerable<string> TrainProblems { get; set; } = new List<string>();
        [Option("test-problems", Required = true, HelpText = "Problems to test on later.")]
        public IEnumerable<string> TestProblems { get; set; } = new List<string>();
        [Option("tempDir", Required = false, HelpText = "Path where all the intermediate files can be saved.", Default = "temp/train")]
        public string TempPath { get; set; } = "temp/train";
        [Option("output", Required = false, HelpText = "Path where all the output will be saved", Default = "output/train")]
        public string OutputPath { get; set; } = "output/train";
    }
}
