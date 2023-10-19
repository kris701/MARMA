﻿using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using System.Diagnostics;
using Tools;

namespace MetaActions.Test
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
        }

        private static void Run(Options opts)
        {
            opts.DomainPath = PathHelper.RootPath(opts.DomainPath);
            opts.MetaActionsPath = PathHelper.RootPath(opts.MetaActionsPath);
            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);
            ICodeGenerator<INode> codeGenerator = new PDDLCodeGenerator(listener);
            DomainDecl domainDecl = parser.ParseAs<DomainDecl>(new FileInfo(opts.DomainPath));
            List<ActionDecl> metaActions = new List<ActionDecl>();
            foreach (var file in new DirectoryInfo(opts.MetaActionsPath).GetFiles())
                metaActions.Add(parser.ParseAs<ActionDecl>(file));

            RecratePath(opts.TempPath);
            RecratePath(opts.OutputPath);

            foreach (var problem in opts.TestingProblems)
            {
                var rootedProblem = PathHelper.RootPath(problem);
                var name = new FileInfo(problem).Name.Replace(".pddl", "");
                var outPath = Path.Combine(opts.OutputPath, name);
                var tempPath = Path.Combine(opts.TempPath, name);
                RecratePath(outPath);
                RecratePath(tempPath);
                var normalTime = ExecuteAsNormal(opts.DomainPath, rootedProblem, Path.Combine(outPath, "normalPlan.plan"), Path.Combine(tempPath, "normalOutput.sas"));
                var metaTime = ExecuteAsMeta(domainDecl.Copy(), opts.DomainPath, rootedProblem, metaActions, tempPath, outPath, codeGenerator);
                Console.WriteLine($"Normal took: {normalTime}ms");
                Console.WriteLine($"Meta took:   {metaTime}ms");
            }
        }

        private static void RecratePath(string path)
        {
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
            Directory.CreateDirectory(path);
        }

        private static float ExecuteAsNormal(string domain, string problem, string planName, string sasName)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            ArgsCaller fdCaller = new ArgsCaller("python3");
            fdCaller.Arguments.Add(PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"), "");
            fdCaller.Arguments.Add("--alias", "lama-first");
            fdCaller.Arguments.Add("--plan-file", planName);
            fdCaller.Arguments.Add("--sas-file", sasName);
            fdCaller.Arguments.Add(domain, "");
            fdCaller.Arguments.Add(problem, "");
            fdCaller.Run();

            timer.Stop();
            return timer.ElapsedMilliseconds;
        }

        private static float ExecuteAsMeta(DomainDecl domain, string originalDomain, string problem, List<ActionDecl> metaActions, string tempPath, string outPath, ICodeGenerator<INode> codeGenerator)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            // Reformulate domain
            foreach (var act in metaActions)
                domain.Actions.Add(act);
            codeGenerator.Generate(domain, Path.Combine(tempPath, "reformulated_domain.pddl"));

            // Execute with FD
            ExecuteAsNormal(Path.Combine(tempPath, "reformulated_domain.pddl"), problem, Path.Combine(outPath, "metaPlan.plan"), Path.Combine(tempPath, "metaOutput.sas"));

            // Reconstruct plan
            ArgsCaller reconstructionFixer = ArgsCallerBuilder.GetRustRunner("reconstruction");
            reconstructionFixer.Arguments.Add("-d", originalDomain);
            reconstructionFixer.Arguments.Add("-p", problem);
            reconstructionFixer.Arguments.Add("-m", Path.Combine(tempPath, "reformulated_domain.pddl"));
            reconstructionFixer.Arguments.Add("-s", Path.Combine(outPath, "metaPlan.plan"));
            reconstructionFixer.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
            reconstructionFixer.Arguments.Add("-o", Path.Combine(outPath, "reconstructedPlan.plan"));
            reconstructionFixer.Run();

            timer.Stop();
            return timer.ElapsedMilliseconds;
        }
    }
}