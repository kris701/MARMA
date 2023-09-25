using CommandLine;
using CommandLine.Text;
using PlanSampleGenerator;
using System;
using System.Runtime.InteropServices;
using Tools;
using Tools.Benchmarks;

namespace Toolchain
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            var parser = new Parser(with => { 
                with.IgnoreUnknownArguments = true;
                with.AutoHelp = true;
                with.AutoVersion = true;
                with.HelpWriter = Parser.Default.Settings.HelpWriter;
            });

            // Plan Sample Generator
            parser.ParseArguments<PlanSampleGeneratorOptions>(args)
              .WithParsed(PlanSampleGenerator.Program.RunPlanSampleGeneration)
              .WithNotParsed(HandleParseError);
        }
    }
}