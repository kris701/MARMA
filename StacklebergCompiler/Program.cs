using System;
using CommandLine;
using System.Diagnostics;
using System.Text;
using PDDLSharp;
using Tools;
using PDDLSharp.Parsers;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.Problem;
using PDDLSharp.Models.Domain;
using PDDLSharp.CodeGenerators;
using PDDLSharp.Analysers;
using PDDLSharp.Models;

namespace StacklebergCompiler
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<StacklebergCompilerOptions>(args)
              .WithParsed(RunStacklebergCompiler)
              .WithNotParsed(HandleParseError);
        }

        public static void RunStacklebergCompiler(StacklebergCompilerOptions opts)
        {
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.MetaActionFile = PathHelper.RootPath(opts.MetaActionFile);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Verifying paths...", ConsoleColor.DarkGray);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);
            if (!File.Exists(opts.DomainFilePath))
                throw new FileNotFoundException($"Domain file not found: {opts.DomainFilePath}");
            if (!File.Exists(opts.ProblemFilePath))
                throw new FileNotFoundException($"Problem file not found: {opts.ProblemFilePath}");
            if (!File.Exists(opts.MetaActionFile))
                throw new FileNotFoundException($"Meta action file not found: {opts.MetaActionFile}");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Parsing files...", ConsoleColor.DarkGray);
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);

            var domain = parser.ParseAs<DomainDecl>(opts.DomainFilePath);
            var problem = parser.ParseAs<ProblemDecl>(opts.ProblemFilePath);
            var metaAction = parser.ParseAs<ActionDecl>(opts.MetaActionFile);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating conditional domain/problem...", ConsoleColor.DarkGray);
            ConditionalEffectCompiler compiler = new ConditionalEffectCompiler();
            var conditionalDecl = compiler.GenerateConditionalEffects(domain, problem, metaAction);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating abstracted domain/problem...", ConsoleColor.DarkGray);
            ConditionalEffectSimplifyer abstractor = new ConditionalEffectSimplifyer();
            var abstractedConditionalDec = abstractor.AbstractConditionalEffects(conditionalDecl.Domain, conditionalDecl.Problem);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting files...", ConsoleColor.DarkGray);
            ICodeGenerator generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            generator.Generate(conditionalDecl.Domain, Path.Combine(opts.OutputPath, "conditional_domain.pddl"));
            generator.Generate(conditionalDecl.Problem, Path.Combine(opts.OutputPath, "conditional_problem.pddl"));
            generator.Generate(abstractedConditionalDec.Domain, Path.Combine(opts.OutputPath, "abstracted_domain.pddl"));
            generator.Generate(abstractedConditionalDec.Problem, Path.Combine(opts.OutputPath, "abstracted_problem.pddl"));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }
    }
}