using CommandLine;
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

            ConsoleHelper.WriteLineColor($"Starting to learn meta actions...", ConsoleColor.Blue);

            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            var domains = PathHelper.ResolveWildcards(opts.Domains.ToList());
            var trainProblems = PathHelper.ResolveWildcards(opts.TrainProblems.ToList());
            var testProblems = PathHelper.ResolveWildcards(opts.TestProblems.ToList());

            List<Task> runTasks = new List<Task>();
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

                runTasks.Add(new Task(() => new LearningTask().LearnDomain(tempPath, outPath, domain, domainTrainProblems, domainTestProblems), token));
            }

            Parallel.ForEach(runTasks, task => task.Start());
            Task.WaitAll(runTasks.ToArray());
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
        }
    }
}