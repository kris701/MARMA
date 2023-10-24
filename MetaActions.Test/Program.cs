using CommandLine;
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
            ConsoleHelper.WriteLineColor($"Starting meta action testing", ConsoleColor.Blue);

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

            ConsoleHelper.WriteLineColor($"There is a total of {opts.TestingProblems.Count()} problems to test with.", ConsoleColor.Blue);

            int count = 1;
            float totalNormalTime = 0;
            float totalMetaTime = 0;
            foreach (var problem in opts.TestingProblems)
            {
                ConsoleHelper.WriteLineColor($"\tTesting problem {count++} out of {opts.TestingProblems.Count()}", ConsoleColor.Magenta);
                var rootedProblem = PathHelper.RootPath(problem);
                var name = new FileInfo(problem).Name.Replace(".pddl", "");
                var outPath = Path.Combine(opts.OutputPath, name);
                var tempPath = Path.Combine(opts.TempPath, name);
                RecratePath(outPath);
                RecratePath(tempPath);
                ConsoleHelper.WriteLineColor($"\tExecuting normal problem", ConsoleColor.Magenta);
                var normalTime = ExecuteAsNormal(opts.DomainPath, rootedProblem, Path.Combine(outPath, "normalPlan.plan"), Path.Combine(tempPath, "normalOutput.sas"));
                ConsoleHelper.WriteLineColor($"\tExecuting meta problem", ConsoleColor.Magenta);
                var metaTime = ExecuteAsMeta(domainDecl.Copy(), opts.DomainPath, rootedProblem, metaActions, tempPath, outPath, codeGenerator);
                PrintResult(normalTime, metaTime);
                totalNormalTime += normalTime;
                totalMetaTime += metaTime;
            }
            ConsoleHelper.WriteLineColor($"Testing finished!", ConsoleColor.Green);
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

            ArgsCaller fdCaller = ArgsCallerBuilder.GetGenericRunner("python3");
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

        private static void PrintResult(float normalTime, float metaTime)
        {
            ConsoleHelper.WriteColor($"\tNormal took: ", ConsoleColor.Magenta);
            if (normalTime < metaTime)
                ConsoleHelper.WriteColor($"{normalTime}", ConsoleColor.Green);
            else
                ConsoleHelper.WriteColor($"{normalTime}", ConsoleColor.Red);
            ConsoleHelper.WriteLineColor($"ms", ConsoleColor.Magenta);
            ConsoleHelper.WriteColor($"\tMeta took:   ", ConsoleColor.Magenta);
            if (normalTime < metaTime)
                ConsoleHelper.WriteColor($"{metaTime}", ConsoleColor.Red);
            else
                ConsoleHelper.WriteColor($"{metaTime}", ConsoleColor.Green);
            ConsoleHelper.WriteLineColor($"ms", ConsoleColor.Magenta);
        }
    }
}