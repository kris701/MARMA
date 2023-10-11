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
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.MacroActionPath = PathHelper.RootPath(opts.MacroActionPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Verifying paths...", ConsoleColor.DarkGray);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);
            if (!File.Exists(opts.DomainFilePath))
                throw new FileNotFoundException($"Domain file not found: {opts.DomainFilePath}");
            if (!File.Exists(opts.ProblemFilePath))
                throw new FileNotFoundException($"Problem file not found: {opts.ProblemFilePath}");
            if (!Directory.Exists(opts.MacroActionPath))
                throw new FileNotFoundException($"Macro action path not found: {opts.MacroActionPath}");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Parsing domain and problem...", ConsoleColor.DarkGray);
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);

            var domain = parser.ParseAs<DomainDecl>(opts.DomainFilePath);
            var problem = parser.ParseAs<ProblemDecl>(opts.ProblemFilePath);
            var decl = new PDDLDecl(domain, problem);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Parsing meta actions...", ConsoleColor.DarkGray);
            List<ActionDecl> macros = new List<ActionDecl>();
            foreach(var file in Directory.GetFiles(opts.MacroActionPath))
                if (file.ToLower().EndsWith(".pddl"))
                    macros.Add(parser.ParseAs<ActionDecl>(file));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            List<ActionDecl> metaActions = new List<ActionDecl>();

            ConsoleHelper.WriteLineColor("Generating 'Remove Precondition Parameters' meta actions...", ConsoleColor.DarkGray);
            ICandidateGenerator cpre = new RemovePreconditionParameters(decl);
            metaActions.AddRange(cpre.Generate(macros));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating 'Remove Effect Parameters' meta actions...", ConsoleColor.DarkGray);
            ICandidateGenerator ceff = new RemoveEffectParameters(decl);
            metaActions.AddRange(ceff.Generate(macros));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating 'Remove Additional Effects' meta actions...", ConsoleColor.DarkGray);
            ICandidateGenerator cinv = new RemoveAdditionalEffects(decl);
            metaActions.AddRange(cinv.Generate(macros));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting files...", ConsoleColor.DarkGray);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);

            ICodeGenerator<INode> generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            foreach(var metaAction in metaActions)
                generator.Generate(metaAction, Path.Combine(opts.OutputPath, $"meta_{metaAction.Name}.pddl"));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}