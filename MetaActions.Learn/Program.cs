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

            var domains = ResolveWildcards(opts.Domains.ToList());
            var problems = ResolveWildcards(opts.Problems.ToList());

            List<Task> runTasks = new List<Task>();
            CancellationToken token = new CancellationToken();

            foreach(var domain in domains)
            {
                if (domain.Directory == null)
                    throw new ArgumentNullException();
                var domainName = domain.Directory.Name;
                var domainProblems = problems.Where(x => x.FullName.Contains(domainName)).ToList();

                var tempPath = Path.Combine(opts.TempPath, domainName);
                var outPath = Path.Combine(opts.OutputPath, domainName);

                runTasks.Add(new Task(() => new LearningTask().LearnDomain(tempPath, outPath, domain, domainProblems), token));
            }

            Parallel.ForEach(runTasks, task => task.Start());
            Task.WaitAll(runTasks.ToArray());
            ConsoleHelper.WriteLineColor($"Done!", ConsoleColor.Green);
        }

        private static List<FileInfo> ResolveWildcards(List<string> items)
        {
            List<FileInfo> returnFiles = new List<FileInfo>();

            foreach(var item in items)
            {
                if (item.Contains('*'))
                {
                    List<string> subItems = new List<string>();
                    var currentWildcard = item.IndexOf('*');
                    var preChar = item.LastIndexOf('/', currentWildcard - 1);
                    var postChar = item.IndexOf('/', currentWildcard);
                    if (preChar == currentWildcard - 1 && postChar == currentWildcard + 1)
                    {
                        var route = item.Substring(0, currentWildcard);
                        var remaining = item.Substring(currentWildcard + 2);
                        foreach (var option in new DirectoryInfo(route).GetDirectories())
                            subItems.Add(Path.Combine(route, option.Name, remaining));

                        returnFiles.AddRange(ResolveWildcards(subItems));
                    }
                    else if (postChar == -1)
                    {
                        var route = item.Substring(0, preChar);
                        if (Directory.Exists(route))
                        {
                            var remaining = item.Substring(preChar + 1);
                            foreach (var option in Directory.GetFiles(route, remaining))
                                subItems.Add(option);

                            returnFiles.AddRange(ResolveWildcards(subItems));
                        }
                    }
                }
                else
                {
                    var target = PathHelper.RootPath(item);
                    if (File.Exists(target))
                        returnFiles.Add(new FileInfo(target));
                }
            }

            return returnFiles;
        }
    }
}