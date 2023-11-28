﻿using CommandLine;
using MetaActions.Train;
using MetaActions.Train.Tools;
using MetaActions.Train.Trainers;
using System.Diagnostics.Metrics;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        private static int _ticked = 1;
        private static int _totalTasks = 1;
        private static int _finishedTasks = 0;
        static int Main(string[] args)
        {
            var parser = new CommandLine.Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult.WithNotParsed(errs => DisplayHelp(parserResult, errs));
            parserResult.WithParsed(Run);
            return 0;
        }

        private static void Run(Options opts)
        {
            var timestamp = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            if (opts.Rebuild)
            {
                ConsoleHelper.WriteLineColor($"Building Toolchain", ConsoleColor.Blue);
                PreBuilder.BuildToolchain();
                ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
            }

            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            PathHelper.RecratePath(opts.TempPath);
            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor($"Generating Training Tasks", ConsoleColor.Blue);
            var trainingTasks = GenerateTasks(opts);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Executing Training Tasks", ConsoleColor.Blue);
            ConsoleHelper.WriteLineColor($"Time limit: {opts.TimeLimit}m", ConsoleColor.Blue);
            var runReports = ExecuteTasks(trainingTasks, opts.Multitask);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Generating identity file...", ConsoleColor.Blue);
            File.WriteAllText(Path.Combine(opts.OutputPath, $"{timestamp}.json"), JsonSerializer.Serialize(opts));
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Generating training information file...", ConsoleColor.Blue);
            GenerateTrainCSV(runReports, Path.Combine(opts.OutputPath, $"{timestamp}.csv"));
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Compressing testing dataset...", ConsoleColor.Blue);
            ZipFile.CreateFromDirectory(opts.OutputPath, Path.Combine(opts.TempPath, $"testing-set-{timestamp}.zip"));
            File.Move(Path.Combine(opts.TempPath, $"testing-set-{timestamp}.zip"), Path.Combine(opts.OutputPath, $"testing-set-{timestamp}.zip"));
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
        }

        private static List<ITrainer> GenerateTasks(Options opts)
        {
            ConsoleHelper.WriteLineColor($"\tResolving input wildcards...", ConsoleColor.Magenta);
            var domains = PathHelper.ResolveFileWildcards(opts.Domains.ToList());
            if (domains.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Domain file not found!");
            var trainProblems = PathHelper.ResolveFileWildcards(opts.TrainProblems.ToList());
            if (trainProblems.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Train problem file not found!");
            var testProblems = PathHelper.ResolveFileWildcards(opts.TestProblems.ToList());
            if (testProblems.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Test problem file not found!");

            List<ITrainer> runTasks = new List<ITrainer>();
            int count = 1;
            foreach (var domain in domains)
            {
                if (domain.Directory == null)
                    throw new ArgumentNullException();
                var domainName = domain.Directory.Name;
                ConsoleHelper.WriteLineColor($"\tGenerating Task for domain {domainName} [{count++}/{domains.Count}]", ConsoleColor.Magenta);
                var domainTrainProblems = trainProblems.Where(x => x.FullName.Contains(domainName)).ToList();
                var domainTestProblems = testProblems.Where(x => x.FullName.Contains(domainName)).ToList();

                var tempPath = Path.Combine(opts.TempPath, domainName);
                var outPath = Path.Combine(opts.OutputPath, domainName);
                runTasks.Add(new MARMATrainer(
                            domainName,
                            domain,
                            domainTrainProblems,
                            domainTestProblems,
                            TimeSpan.FromMinutes(opts.TimeLimit),
                            tempPath,
                            outPath,
                            opts.VerificationStrategy,
                            opts.GenerationStrategy
                            ));
            }

            runTasks.Shuffle();

            return runTasks;
        }

        private static List<RunReport> ExecuteTasks(List<ITrainer> runTasks, bool multitask)
        {
            var runReports = new List<RunReport>();
            _totalTasks = runTasks.Count;
            StartTimeLeftTimer();
            if (multitask)
            {
                var tasks = new List<Task<RunReport>>();
                foreach (var task in runTasks)
                    tasks.Add(task.RunTask());
                foreach (var task in tasks)
                    task.Start();

                while (tasks.Count > 0)
                {
                    try
                    {
                        var resultTask = Task.WhenAny(tasks).Result;
                        tasks.Remove(resultTask);
                        var result = resultTask.Result;
                        runReports.Add(result);
                        _finishedTasks++;
                        if (!result.TimedOut)
                            ConsoleHelper.WriteLineColor($"Training for '{result.TaskID}' complete! {TaskStatus()}", ConsoleColor.Green);
                        else
                            ConsoleHelper.WriteLineColor($"Task '{result.TaskID}' canceled! {TaskStatus()}", ConsoleColor.Yellow);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteLineColor($"Something failed in the training!", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                        if (ex.StackTrace != null)
                            ConsoleHelper.WriteLineColor(ex.StackTrace, ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor($"", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor($"Killing tasks...!", ConsoleColor.Red);
                        foreach (var cancel in runTasks)
                            cancel.CancellationToken.Cancel();
                    }
                }
            }
            else
            {
                foreach (var task in runTasks)
                {
                    try
                    {
                        var resultTask = task.RunTask();
                        resultTask.Start();
                        resultTask.Wait();
                        var result = resultTask.Result;
                        runReports.Add(result);
                        _finishedTasks++;
                        if (!result.TimedOut)
                            ConsoleHelper.WriteLineColor($"Training for '{result.TaskID}' complete! {TaskStatus()}", ConsoleColor.Green);
                        else
                            ConsoleHelper.WriteLineColor($"Task '{result.TaskID}' canceled! {TaskStatus()}", ConsoleColor.Yellow);
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteLineColor($"Something failed in the training!", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                        foreach (var cancel in runTasks)
                            cancel.CancellationToken.Cancel();
                    }
                }
            }
            return runReports;
        }

        private static void StartTimeLeftTimer()
        {
            var timeUpdateTimer = new System.Timers.Timer();
            timeUpdateTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
            timeUpdateTimer.Elapsed += (s, e) =>
            {
                ConsoleHelper.WriteLineColor($"Time passed: {_ticked++}m {TaskStatus()}", ConsoleColor.Blue);
            };
            timeUpdateTimer.Start();
        }

        private static string TaskStatus() 
        {
            return $"[{_finishedTasks} finished out of {_totalTasks} tasks, {Math.Round(100 * ((double)_finishedTasks / (double)_totalTasks), 0)}%]";
        }

        private static void GenerateTrainCSV(List<RunReport> runReports, string resultFile)
        {
            var sb = new StringBuilder();
            var header = "";
            foreach (PropertyInfo p in typeof(RunReport).GetProperties())
                header += $"{p.Name},";
            header = header.Remove(header.Length - 1);
            sb.AppendLine(header);
            foreach (var report in runReports)
            {
                var line = "";
                foreach (PropertyInfo p in typeof(RunReport).GetProperties())
                    line += $"{p.GetValue(report, null)},";
                line = line.Remove(line.Length - 1);
                sb.AppendLine(line);
            }

            File.WriteAllText(resultFile, sb.ToString());
        }
    }   
}