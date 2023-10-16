using System;
using CommandLine;
using System.Diagnostics;
using System.Text;
using PDDLSharp;
using PDDLSharp.Parsers;
using PDDLSharp.ErrorListeners;
using PDDLSharp.CodeGenerators;
using PDDLSharp.Analysers;
using PDDLSharp.Models;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Models.PDDL;
using Tools;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.Parsers.PDDL;

namespace MetaActionGenerator
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<MetaActionGeneratorOptions>(args)
              .WithParsed(GenerateMetaActions)
              .WithNotParsed(HandleParseError);
        }

        public static void GenerateMetaActions(MetaActionGeneratorOptions opts)
        {
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.MacroActionPath = PathHelper.RootPath(opts.MacroActionPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Verifying paths...", ConsoleColor.DarkGray);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);
            if (!File.Exists(opts.DomainFilePath))
                throw new FileNotFoundException($"Domain file not found: {opts.DomainFilePath}");
            if (!Directory.Exists(opts.MacroActionPath))
                throw new FileNotFoundException($"Macro action path not found: {opts.MacroActionPath}");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Parsing domain...", ConsoleColor.DarkGray);
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);

            var domain = parser.ParseAs<DomainDecl>(new FileInfo(opts.DomainFilePath));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Parsing meta actions...", ConsoleColor.DarkGray);
            List<ActionDecl> macros = new List<ActionDecl>();
            foreach(var file in Directory.GetFiles(opts.MacroActionPath))
                if (file.ToLower().EndsWith(".pddl"))
                    macros.Add(parser.ParseAs<ActionDecl>(new FileInfo(file)));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            List<ActionDecl> metaActions = new List<ActionDecl>();

            ConsoleHelper.WriteLineColor("Generating 'Remove Precondition Parameters' meta actions...", ConsoleColor.DarkGray);
            ICandidateGenerator cpre = new RemovePreconditionParameters(domain);
            metaActions.AddRange(cpre.Generate(macros));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating 'Remove Effect Parameters' meta actions...", ConsoleColor.DarkGray);
            ICandidateGenerator ceff = new RemoveEffectParameters(domain);
            metaActions.AddRange(ceff.Generate(macros));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating 'Remove Additional Effects' meta actions...", ConsoleColor.DarkGray);
            ICandidateGenerator cinv = new RemoveAdditionalEffects(domain);
            metaActions.AddRange(cinv.Generate(macros));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Removing duplicate meta actions...", ConsoleColor.DarkGray);
            int preCount = metaActions.Count;
            metaActions = metaActions.DistinctBy(x => x.GetHashCode()).ToList();
            ConsoleHelper.WriteLineColor($"Removed {preCount - metaActions.Count} actions out of {preCount} [{100 - Math.Round(((double)metaActions.Count / (double)preCount) * 100,0)}%]", ConsoleColor.DarkGray);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Sanetizing meta actions...", ConsoleColor.DarkGray);
            preCount = metaActions.Count;
            metaActions.RemoveAll(x => 
                (x.Preconditions is IWalkable preWalke && preWalke.Count() == 0) ||
                (x.Effects is IWalkable effWalk && effWalk.Count() == 0)
                );
            ConsoleHelper.WriteLineColor($"Removed {preCount - metaActions.Count} actions out of {preCount} [{100 - Math.Round(((double)metaActions.Count / (double)preCount) * 100, 0)}%]", ConsoleColor.DarkGray);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting files...", ConsoleColor.DarkGray);
            ConsoleHelper.WriteLineColor($"A total of {metaActions.Count} meta action was found.", ConsoleColor.DarkGray);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);
            else
                foreach (FileInfo file in new DirectoryInfo(opts.OutputPath).GetFiles())
                    file.Delete();

            ICodeGenerator<INode> generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            int counter = 0;
            foreach(var metaAction in metaActions)
                generator.Generate(metaAction, Path.Combine(opts.OutputPath, $"meta_{counter++}.pddl"));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}