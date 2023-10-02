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
            IErrorListener listener = new ErrorListener();
            IParser parser = new PDDLParser(listener);

            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.MetaActionFile = PathHelper.RootPath(opts.MetaActionFile);

            var domain = parser.ParseAs<DomainDecl>(opts.DomainFilePath);
            var problem = parser.ParseAs<ProblemDecl>(opts.ProblemFilePath);
            var metaAction = parser.ParseAs<ActionDecl>(opts.MetaActionFile);

            ConditionalEffectCompiler compiler = new ConditionalEffectCompiler();
            var conditionalDecl = compiler.GenerateConditionalEffects(domain, problem, metaAction);

            ConditionalEffectAbstractor abstractor = new ConditionalEffectAbstractor();
            var abstractedConditionalDec = abstractor.AbstractConditionalEffects(conditionalDecl.Domain, conditionalDecl.Problem);

            ICodeGenerator generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;
            generator.Generate(abstractedConditionalDec.Domain, "new_domain.pddl");
            generator.Generate(abstractedConditionalDec.Problem, "new_problem.pddl");
        }
    }
}