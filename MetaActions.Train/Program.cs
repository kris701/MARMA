using CommandLine;
using MetaActions.Train;
using MetaActions.Train.Trainers;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        private static CancellationTokenSource _tokenSource = new CancellationTokenSource();

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
            ExecuteTasks(trainingTasks, opts.Multitask, opts.OutputPath);
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Generating identity file...", ConsoleColor.Blue);
            File.WriteAllText(Path.Combine(opts.OutputPath, $"{timestamp}.json"), JsonSerializer.Serialize(opts));
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Compressing testing dataset...", ConsoleColor.Blue);
            ZipFile.CreateFromDirectory(opts.OutputPath, Path.Combine(opts.TempPath, $"testing-set-{timestamp}.zip"));
            File.Move(Path.Combine(opts.TempPath, $"testing-set-{timestamp}.zip"), Path.Combine(opts.OutputPath, $"testing-set-{timestamp}.zip"));
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
        }

        private static List<ITrainer> GenerateTasks(Options opts)
        {
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
            foreach (var domain in domains)
            {
                if (domain.Directory == null)
                    throw new ArgumentNullException();
                var domainName = domain.Directory.Name;
                ConsoleHelper.WriteLineColor($"\tGenerating Task for domain {domainName}", ConsoleColor.Magenta);
                var domainTrainProblems = trainProblems.Where(x => x.FullName.Contains(domainName)).ToList();
                var domainTestProblems = testProblems.Where(x => x.FullName.Contains(domainName)).ToList();

                var tempPath = Path.Combine(opts.TempPath, domainName);
                var outPath = Path.Combine(opts.OutputPath, domainName);

                switch (opts.TrainingMethod)
                {
                    case Options.TrainingMethods.CSMMacros:
                        runTasks.Add(new CSMTrainer(
                            domainName,
                            domain,
                            domainTrainProblems,
                            domainTestProblems,
                            TimeSpan.FromMinutes(opts.TimeLimit),
                            tempPath,
                            outPath,
                            opts.Useful,
                            _tokenSource
                            ));
                        break;
                    case Options.TrainingMethods.PDDLSharpMacros:
                        runTasks.Add(new PDDLSharpTrainer(
                            domainName,
                            domain,
                            domainTrainProblems,
                            domainTestProblems,
                            TimeSpan.FromMinutes(opts.TimeLimit),
                            tempPath,
                            outPath,
                            opts.Useful,
                            _tokenSource
                            ));
                        break;
                    default:
                        throw new Exception("Training method not implemented");
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

        private static void ExecuteTasks(List<ITrainer> runTasks, bool multitask, string outPath)
        {
            if (multitask)
            {
                int counter = 1;
                var tasks = new List<Task<RunReport?>>();
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

                        if (result != null)
                        {
                            ConsoleHelper.WriteLineColor($"Test for [{result.TaskID}] complete! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Green);
                        }
                        else
                            ConsoleHelper.WriteLineColor($"Task canceled! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Yellow);
                    }
                    catch (Exception ex)
                    {
                        _tokenSource.Cancel();
                        ConsoleHelper.WriteLineColor($"Something failed in the training!", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor($"", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor($"Killing tasks...!", ConsoleColor.Red);
                    }
                }
            }
            else
            {
                int counter = 1;
                foreach (var task in runTasks)
                {
                    try
                    {
                        if (_tokenSource.IsCancellationRequested)
                            break;
                        var resultTask = task.RunTask();
                        resultTask.Start();
                        resultTask.Wait();
                        var result = resultTask.Result;
                        if (result != null)
                        {
                            ConsoleHelper.WriteLineColor($"Test for [{result.TaskID}] complete! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Green);
                        }
                        else
                            ConsoleHelper.WriteLineColor($"Task canceled! [{Math.Round(100 * ((double)counter++ / (double)runTasks.Count), 0)}%]", ConsoleColor.Yellow);
                    }
                    catch (Exception ex)
                    {
                        _tokenSource.Cancel();
                        ConsoleHelper.WriteLineColor($"Something failed in the training!", ConsoleColor.Red);
                        ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                    }
                }
            }
        }
    }
}