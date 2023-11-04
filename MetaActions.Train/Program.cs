using CommandLine;
using System.IO.Compression;
using System.Threading;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        private static void Run(Options opts)
        {
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

            var domains = PathHelper.ResolveFileWildcards(opts.Domains.ToList());
            if (domains.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Domain file not found!");
            var trainProblems = PathHelper.ResolveFileWildcards(opts.TrainProblems.ToList());
            if (trainProblems.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Train problem file not found!");
            var testProblems = PathHelper.ResolveFileWildcards(opts.TestProblems.ToList());
            if (testProblems.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Test problem file not found!");

            ConsoleHelper.WriteLineColor($"Starting to learn meta actions of {domains.Count} domains...", ConsoleColor.Blue);

            List<Task<string>> runTasks = new List<Task<string>>();
            CancellationToken token = new CancellationToken();

            foreach(var domain in domains)
            {
                if (domain.Directory == null)
                    throw new ArgumentNullException();
                var domainName = domain.Directory.Name;
                var domainTrainProblems = trainProblems.Where(x => x.FullName.Contains(domainName)).ToList();
                var domainTestProblems = testProblems.Where(x => x.FullName.Contains(domainName)).ToList();

                var tempPath = Path.Combine(opts.TempPath, domainName);
                var outPath = Path.Combine(opts.OutputPath, domainName);

                runTasks.Add(new Task<string>(() => new LearningTask().LearnDomain(tempPath, outPath, domain, domainTrainProblems, domainTestProblems), token));
            }

            try
            {
                Parallel.ForEach(runTasks, task => task.Start());
                int preCount = runTasks.Count;
                while (runTasks.Count > 0)
                {
                    var task = Task.WhenAny(runTasks).Result;
                    runTasks.Remove(task);
                    ConsoleHelper.WriteLineColor($"Training for domain {task.Result} complete! [{Math.Round(100 - (100 * ((double)runTasks.Count / (double)preCount)),0)}%]", ConsoleColor.Green);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLineColor($"Something failed in the learning!", ConsoleColor.Red);
                ConsoleHelper.WriteLineColor(ex.Message, ConsoleColor.Red);
                return;
            }
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor($"Compressing testing dataset...", ConsoleColor.Blue);

            ZipFile.CreateFromDirectory(opts.OutputPath, Path.Combine(opts.TempPath, "testing-set.zip"));
            File.Move(Path.Combine(opts.TempPath, "testing-set.zip"), Path.Combine(opts.OutputPath, "testing-set.zip"));

            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
        }
    }
}