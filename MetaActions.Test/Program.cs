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
using System.Text.Json;
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
        private static string _tempTempPath = "temp";

        private static void Run(Options opts)
        {
            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            opts.OutputPath = Path.Combine(opts.OutputPath, DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss"));
            opts.DataFile = PathHelper.ResolveFileWildcards(new List<string>() { opts.DataFile })[0].FullName;
            _tempDataPath = Path.Combine(opts.TempPath, _tempDataPath);
            _tempTempPath = Path.Combine(opts.TempPath, _tempTempPath);

            WriteToConsoleAndLog($"Recreating paths...", ConsoleColor.Blue);
            PathHelper.RecratePath(opts.OutputPath);
            PathHelper.RecratePath(_tempTempPath);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            WriteToConsoleAndLog($"Setting up run log...", ConsoleColor.Blue);
            SetupRunLog(opts.OutputPath);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            if (opts.Rebuild)
            {
                WriteToConsoleAndLog($"Rebuilding toolchain...", ConsoleColor.Blue);
                if (ArgsCallerBuilder.GetRustBuilder("reconstruction").Run() != 0)
                    throw new Exception("Reconstruction build failed!");
                WriteToConsoleAndLog($"Done!", ConsoleColor.Green);
            }

            WriteToConsoleAndLog($"Extracting testing data...", ConsoleColor.Blue);
            ExtractTestData(opts.DataFile);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            WriteToConsoleAndLog($"Copying configurations...", ConsoleColor.Blue);
            CopyConfigurations(opts);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            WriteToConsoleAndLog($"Initializing tests...", ConsoleColor.Blue);
            var runTasks = GenerateTasks(opts);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            WriteToConsoleAndLog($"Setting up run report...", ConsoleColor.Blue);
            SetupRunReport(opts.OutputPath);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            WriteToConsoleAndLog($"Executing a total of {runTasks.Count} tasks...", ConsoleColor.Blue);
            ExecuteTasks(runTasks, opts.MultiTask, opts.OutputPath);
            WriteToConsoleAndLog($"Done!", ConsoleColor.Green);

            WriteToConsoleAndLog($"Testing suite finished!", ConsoleColor.Green);
        }

        private static string _logPath = "";
        private static void SetupRunLog(string outPath)
        {
            _logPath = Path.Combine(outPath, "log.txt");
            File.WriteAllText(_logPath, $"Setting up log...{Environment.NewLine}");
        }

        private static void ExtractTestData(string dataFile)
        {
            var config = GetConfig(_tempDataPath);
            if (config != null)
                if (dataFile.Contains(config.Name.Replace(".json","")))
                    return;

            PathHelper.RecratePath(_tempDataPath);
            ZipFile.ExtractToDirectory(dataFile, _tempDataPath);
        }

        private static FileInfo? GetConfig(string dir)
        {
            if (!Directory.Exists(dir))
                return null;
            foreach (var file in new DirectoryInfo(dir).GetFiles())
                if (file.Extension == ".json")
                    return file;
            return null;
        }

        private static void CopyConfigurations(Options opts)
        {
            var trainConfig = GetConfig(_tempDataPath);
            if (trainConfig != null)
                File.Copy(trainConfig.FullName, Path.Combine(opts.OutputPath, "train-config.json"));
            File.WriteAllText(Path.Combine(opts.OutputPath, "test-config.json"), JsonSerializer.Serialize(opts));
        }

        private static List<TestingTask> GenerateTasks(Options opts)
        {
            List<TestingTask> runTasks = new List<TestingTask>();
            foreach (var domain in new DirectoryInfo(_tempDataPath).GetDirectories())
            {
                var domainName = domain.Name;

                PathHelper.RecratePath(Path.Combine(_tempTempPath, domainName));
                PathHelper.RecratePath(Path.Combine(opts.OutputPath, domainName));

                var normalDomain = new FileInfo(Path.Combine(domain.FullName, "data", "domain.pddl"));
                var metaDomain = new FileInfo(Path.Combine(domain.FullName, "data", "metaDomain.pddl"));

                var allProblems = new DirectoryInfo(Path.Combine(domain.FullName, "data")).GetFiles().ToList();
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
                        Path.Combine(_tempTempPath, domainName, $"{problemName}.sas"),
                        Options.ReconstructionMethods.None,
                        Path.Combine(domain.FullName, "cache")));
                    runTasks.Add(new TestingTask(
                        opts.TimeLimit,
                        opts.Alias,
                        normalDomain,
                        metaDomain,
                        problem,
                        Path.Combine(opts.OutputPath, domainName, $"{problemName}_reconstructed.plan"),
                        Path.Combine(opts.OutputPath, domainName, $"{problemName}_meta.plan"),
                        Path.Combine(_tempTempPath, domainName, $"{problemName}_meta.sas"),
                        opts.ReconstructionMethod,
                        Path.Combine(domain.FullName, "cache")));
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

        private static void ExecuteTasks(List<TestingTask> runTasks, bool multitask, string outPath)
        {
            var tokenSource = new CancellationTokenSource();
            if (multitask)
            {
                int counter = 1;
                var tasks = new List<Task>();
                foreach (var task in runTasks)
                {
                    tasks.Add(Task.Run(() => {
                        try
                        {
                            if (tokenSource.IsCancellationRequested)
                                return;
                            var result = task.RunTest(tokenSource);
                            AppendToReport(result, outPath);
                            if (tokenSource.IsCancellationRequested)
                                WriteToConsoleAndLog($"Test for [{result.Domain}, {result.Problem}] canceled! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Red);
                            else
                                WriteToConsoleAndLog($"Test for [{result.Domain}, {result.Problem}] complete! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Green);
                        }
                        catch (Exception ex)
                        {
                            tokenSource.Cancel();
                            WriteToConsoleAndLog($"Something failed in the testing;", ConsoleColor.Red);
                            WriteToConsoleAndLog(ex.Message, ConsoleColor.Red);
                            WriteToConsoleAndLog($"", ConsoleColor.Red);
                            WriteToConsoleAndLog($"Killing tasks...!", ConsoleColor.Red);
                        }
                        return;
                    }));
                }
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
                        AppendToReport(result, outPath);
                        WriteToConsoleAndLog($"Test for [{result.Domain}, {result.Problem}] complete! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Green);
                    }
                    catch (Exception ex)
                    {
                        tokenSource.Cancel();
                        WriteToConsoleAndLog($"Something failed in the testing!", ConsoleColor.Red);
                        WriteToConsoleAndLog(ex.Message, ConsoleColor.Red);
                    }
                }
            }
        }

        private static void SetupRunReport(string outPath)
        {
            var csv = new StringBuilder();
            csv.AppendLine("isMeta,domain,problem,searchTime,totalTime,wasSolutionFound");
            File.WriteAllText(Path.Combine(outPath, "results.csv"), csv.ToString());
        }

        private static void AppendToReport(RunReport result, string outPath)
        {
            var line = "";
            if (result.Domain.StartsWith("(meta)"))
                line = $"true,{result.Domain.Replace("(meta) ", "")},{result.Problem},{result.SearchTime},{result.TotalTime},{result.WasSolutionFound}{Environment.NewLine}";
            else
                line = $"false,{result.Domain},{result.Problem},{result.SearchTime},{result.TotalTime},{result.WasSolutionFound}{Environment.NewLine}";
            File.AppendAllText(Path.Combine(outPath, "results.csv"), line);
        }

        public static void WriteToConsoleAndLog(string text, ConsoleColor color)
        {
            ConsoleHelper.WriteLineColor(text, color);
            if (_logPath != "")
                File.AppendAllText(_logPath, $"{text}{Environment.NewLine}");
        }
    }
}