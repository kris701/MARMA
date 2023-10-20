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
            ConsoleHelper.WriteLineColor($"Building Toolchain", ConsoleColor.Blue);
            
            //PreBuilder.BuildToolchain();

            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);

            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            PathHelper.RecratePath(opts.TempPath);
            PathHelper.RecratePath(opts.OutputPath);

            var domains = PathHelper.ResolveWildcards(opts.Domains.ToList());
            domains.RemoveAll(x => x.Directory.Name == "transport");
            var trainProblems = PathHelper.ResolveWildcards(opts.TrainProblems.ToList());
            var testProblems = PathHelper.ResolveWildcards(opts.TestProblems.ToList());

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
                    ConsoleHelper.WriteLineColor($"Training for domain {task.Result} complete! [{100 - (100 * ((double)runTasks.Count / (double)preCount))}%]", ConsoleColor.Green);
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