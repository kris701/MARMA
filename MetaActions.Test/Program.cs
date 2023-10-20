using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using System.Diagnostics;
using System.IO.Compression;
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

        private static string _tempDataPath = "data";

        private static void Run(Options opts)
        {
            if (opts.Rebuild)
            {
                ConsoleHelper.WriteLineColor($"Rebuilding toolchain...", ConsoleColor.Blue);
                ArgsCallerBuilder.GetRustBuilder("reconstruction").Run();
                ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
            }

            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            opts.DataFile = PathHelper.RootPath(opts.DataFile);

            //PathHelper.RecratePath(opts.TempPath);
            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor($"Extracting testing data", ConsoleColor.Blue);
            _tempDataPath = Path.Combine(opts.TempPath, _tempDataPath);
            //ZipFile.ExtractToDirectory(opts.DataFile, _tempDataPath);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Testing suite started", ConsoleColor.Blue);

            List<Task<RunReport>> runTasks = new List<Task<RunReport>>();
            foreach(var domain in new DirectoryInfo(_tempDataPath).GetDirectories())
            {
                var domainName = domain.Name;

                PathHelper.RecratePath(Path.Combine(opts.TempPath, domainName));
                PathHelper.RecratePath(Path.Combine(opts.OutputPath, domainName));

                var normalDomain = new FileInfo(Path.Combine(domain.FullName, "domain.pddl"));
                var metaDomain = new FileInfo(Path.Combine(domain.FullName, "metaDomain.pddl"));

                var allProblems = new DirectoryInfo(domain.FullName).GetFiles().ToList();
                allProblems.RemoveAll(x => x.Name == normalDomain.Name);
                allProblems.RemoveAll(x => x.Name == metaDomain.Name);

                foreach (var problem in allProblems)
                {
                    var problemName = problem.Name.Replace(".pddl","");
                    runTasks.Add(new Task<RunReport>(() => new TestingTask().RunTest(
                        normalDomain.FullName,
                        "",
                        problem.FullName,
                        Path.Combine(opts.OutputPath, domainName, $"{problemName}.plan"),
                        "",
                        Path.Combine(opts.TempPath, domainName, $"{problemName}.sas")
                        )));
                    runTasks.Add(new Task<RunReport>(() => new TestingTask().RunTest(
                        normalDomain.FullName,
                        metaDomain.FullName,
                        problem.FullName,
                        Path.Combine(opts.OutputPath, domainName, $"reconstructed_{problemName}.plan"),
                        Path.Combine(opts.OutputPath, domainName, $"meta_{problemName}.plan"),
                        Path.Combine(opts.TempPath, domainName, $"{problemName}.sas")
                        )));
                }
            }

            try
            {
                Parallel.ForEach(runTasks, task => task.Start());
                int preCount = runTasks.Count;
                while (runTasks.Count > 0)
                {
                    var task = Task.WhenAny(runTasks).Result;
                    runTasks.Remove(task);
                    ConsoleHelper.WriteLineColor($"Training for problem {task.Result.Problem} complete! [{Math.Round(100 - (100 * ((double)runTasks.Count / (double)preCount)), 0)}%]", ConsoleColor.Green);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineColor($"Something failed in the testing!", ConsoleColor.Red);
                ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                return;
            }

            ConsoleHelper.WriteLineColor($"Testing suite finished!", ConsoleColor.Green);
        }
    }
}