using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
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
                if (ArgsCallerBuilder.GetRustBuilder("reconstruction").Run() != 0)
                    throw new Exception("Reconstruction build failed!");
                ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
            }

            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            opts.OutputPath = Path.Combine(opts.OutputPath, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
            opts.DataFile = PathHelper.RootPath(opts.DataFile);

            PathHelper.RecratePath(opts.TempPath);
            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor($"Extracting testing data", ConsoleColor.Blue);
            _tempDataPath = Path.Combine(opts.TempPath, _tempDataPath);
            ZipFile.ExtractToDirectory(opts.DataFile, _tempDataPath);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Initializing tests...", ConsoleColor.Blue);
            var runTasks = GenerateTasks(opts);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Executing a total of {runTasks.Count} tasks...", ConsoleColor.Blue);
            var results = ExecuteTasks(runTasks, opts.MultiTask);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Generating final run report", ConsoleColor.Blue);
            GenerateReport(results, opts.OutputPath);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Testing suite finished!", ConsoleColor.Green);
        }

        private static List<TestingTask> GenerateTasks(Options opts)
        {
            List<TestingTask> runTasks = new List<TestingTask>();
            foreach (var domain in new DirectoryInfo(_tempDataPath).GetDirectories())
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
                    var problemName = problem.Name.Replace(".pddl", "");
                    runTasks.Add(new TestingTask(
                        opts.TimeLimit, 
                        opts.Alias,
                        normalDomain,
                        null,
                        problem,
                        Path.Combine(opts.OutputPath, domainName, $"{problemName}.plan"),
                        "",
                        Path.Combine(opts.TempPath, domainName, $"{problemName}.sas"),
                        opts.ReconstructionMethod));
                    runTasks.Add(new TestingTask(
                        opts.TimeLimit,
                        opts.Alias,
                        normalDomain,
                        metaDomain,
                        problem,
                        Path.Combine(opts.OutputPath, domainName, $"{problemName}_reconstructed.plan"),
                        Path.Combine(opts.OutputPath, domainName, $"{problemName}_meta.plan"),
                        Path.Combine(opts.TempPath, domainName, $"{problemName}_meta.sas"),
                        opts.ReconstructionMethod));
                }
            }

            Shuffle(runTasks);

            return runTasks;
        }

        private static Random rng = new Random();
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static List<RunReport> ExecuteTasks(List<TestingTask> runTasks, bool multitask)
        {
            List<RunReport> results = new List<RunReport>();
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            if (multitask)
            {
                int counter = 1;
                List<Task> tasks = new List<Task>();
                foreach (var task in runTasks)
                {
                    tasks.Add(Task.Run(() => {
                        try
                        {
                            if (tokenSource.IsCancellationRequested)
                                return;
                            var result = task.RunTest(tokenSource);
                            results.Add(result);
                            if (tokenSource.IsCancellationRequested)
                                ConsoleHelper.WriteLineColor($"Test for [{result.Domain}, {result.Problem}] canceled! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Red);
                            else
                                ConsoleHelper.WriteLineColor($"Test for [{result.Domain}, {result.Problem}] complete! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Green);
                        }
                        catch (Exception ex)
                        {
                            tokenSource.Cancel();
                            ConsoleHelper.WriteLineColor($"Something failed in the testing;", ConsoleColor.Red);
                            ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                            ConsoleHelper.WriteLineColor($"", ConsoleColor.Red);
                            ConsoleHelper.WriteLineColor($"Killing tasks...!", ConsoleColor.Red);
                        }
                        return;
                    }));
                }
                tokenSource.Token.Register(() => {
                    foreach (var item in runTasks)
                        item.Kill();
                });
                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                int counter = 1;
                foreach (var task in runTasks)
                {
                    try
                    {
                        if (tokenSource.IsCancellationRequested)
                            break;
                        var result = task.RunTest(tokenSource);
                        results.Add(result);
                        ConsoleHelper.WriteLineColor($"Test for [{result.Domain}, {result.Problem}] complete! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Green);
                    }
                    catch (Exception ex)
                    {
                        tokenSource.Cancel();
                        ConsoleHelper.WriteLineColor($"Something failed in the testing!", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                    }
                }
            }
            return results;
        }

        private static void GenerateReport(List<RunReport> results, string outPath)
        {
            var csv = new StringBuilder();
            csv.AppendLine("isMeta,domain,problem,searchTime,totalTime,wasSolutionFound");
            foreach (var result in results)
            {
                if (result.Domain.StartsWith("(meta)"))
                    csv.AppendLine($"true,{result.Domain.Replace("(meta) ","")},{result.Problem},{result.SearchTime},{result.TotalTime},{result.WasSolutionFound}");
                else
                    csv.AppendLine($"false,{result.Domain},{result.Problem},{result.SearchTime},{result.TotalTime},{result.WasSolutionFound}");
            }

            File.WriteAllText(Path.Combine(outPath, "results.csv"), csv.ToString());
        }
    }
}