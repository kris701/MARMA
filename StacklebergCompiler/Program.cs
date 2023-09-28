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
            IPDDLParser parser = new PDDLParser(listener);

            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.MetaActionFile = PathHelper.RootPath(opts.MetaActionFile);

            var domain = parser.ParseAs<DomainDecl>(opts.DomainFilePath);
            var problem = parser.ParseAs<ProblemDecl>(opts.ProblemFilePath);
            var metaAction = parser.ParseAs<ActionDecl>(opts.MetaActionFile);

            ConditionalEffectCompiler compiler = new ConditionalEffectCompiler();
            var conditionalDomain = compiler.GenerateConditionalEffects(domain, problem, metaAction);

            IPDDLCodeGenerator generator = new PDDLCodeGenerator(listener);
            generator.Generate(conditionalDomain.Domain, "new_domain.pddl");
            generator.Generate(conditionalDomain.Problem, "new_problem.pddl");
        }
    }
}