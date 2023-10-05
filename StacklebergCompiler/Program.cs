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
            // Check and root Paths
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.MetaActionFile = PathHelper.RootPath(opts.MetaActionFile);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);

            IErrorListener listener = new ErrorListener();
            IParser parser = new PDDLParser(listener);

            // Parse files
            var domain = parser.ParseAs<DomainDecl>(opts.DomainFilePath);
            var problem = parser.ParseAs<ProblemDecl>(opts.ProblemFilePath);
            var metaAction = parser.ParseAs<ActionDecl>(opts.MetaActionFile);

            // Generate new problem and domains
            ConditionalEffectCompiler compiler = new ConditionalEffectCompiler();
            var conditionalDecl = compiler.GenerateConditionalEffects(domain, problem, metaAction);
            ConditionalEffectAbstractor abstractor = new ConditionalEffectAbstractor();
            var abstractedConditionalDec = abstractor.AbstractConditionalEffects(conditionalDecl.Domain, conditionalDecl.Problem);

            ICodeGenerator generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            // Generate code
            generator.Generate(conditionalDecl.Domain, Path.Combine(opts.OutputPath, "conditional_domain.pddl"));
            generator.Generate(conditionalDecl.Problem, Path.Combine(opts.OutputPath, "conditional_problem.pddl"));
            generator.Generate(abstractedConditionalDec.Domain, Path.Combine(opts.OutputPath, "abstracted_domain.pddl"));
            generator.Generate(abstractedConditionalDec.Problem, Path.Combine(opts.OutputPath, "abstracted_problem.pddl"));
        }
    }
}