using CommandLine;
using System;
using System.Text;
using Tools;

namespace TestingScriptGenerator
{
    internal class Program : BaseCLI
    {
        private static int _seed = 1633123;
        private static int _timeLimit = 120;
        static int Main(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);
            parserResult.WithNotParsed(errs => DisplayHelp(parserResult, errs));
            parserResult.WithParsed(Run);
            return 0;
        }

        private static void Run(Options opts)
        {
            ConsoleHelper.WriteLineColor($"\tResolving input wildcards...", ConsoleColor.Magenta);
            var domains = PathHelper.ResolveFileWildcards(opts.Domains.ToList());
            if (domains.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Domain file not found!");
            var trainProblems = PathHelper.ResolveFileWildcards(opts.TrainProblems.ToList());
            if (trainProblems.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Train problem file not found!");
            trainProblems = trainProblems.Distinct().ToList();
            var testProblems = PathHelper.ResolveFileWildcards(opts.TestProblems.ToList());
            if (testProblems.Any(x => !File.Exists(x.FullName)))
                throw new FileNotFoundException("Test problem file not found!");
            testProblems = testProblems.Distinct().ToList();

            var sortedTrainProblems = new List<FileInfo>();
            var sortedTestProblems = new List<FileInfo>();
            var rnd = new Random(_seed);
            Console.WriteLine($"{domains.Count} domains in total");
            foreach(var domain in domains)
            {
                if (domain.Directory == null)
                    throw new ArgumentNullException();
                var domainName = domain.Directory.Name;
                var domainTrainProblems = trainProblems.Where(x => x.FullName.Contains(domainName)).ToList();
                var domainTestProblems = testProblems.Where(x => x.FullName.Contains(domainName)).ToList();
                if (domainTrainProblems.Count > 0)
                {
                    for (int i = 0; i < opts.TrainSplit; i++)
                    {
                        var item = domainTrainProblems[rnd.Next(0, domainTrainProblems.Count)];
                        if (!sortedTrainProblems.Any(x => x.FullName == item.FullName))
                            sortedTrainProblems.Add(item);
                        else
                            i--;
                    }
                    sortedTrainProblems = sortedTrainProblems.OrderBy(x => x.Name).ToList();
                }
                foreach (var domainTrainProblem in domainTrainProblems)
                    if (!sortedTrainProblems.Any(x => x.FullName == domainTrainProblem.FullName))
                        sortedTestProblems.Add(domainTrainProblem);
                foreach (var domainTestProblem in domainTestProblems)
                    if (!sortedTrainProblems.Any(x => x.FullName == domainTestProblem.FullName) && !sortedTestProblems.Any(x => x.FullName == domainTestProblem.FullName))
                        sortedTestProblems.Add(domainTestProblem);

                Console.WriteLine($"Domain {domainName} has {sortedTrainProblems.Where(x => x.FullName.Contains(domainName)).ToList().Count} training problems and {sortedTestProblems.Where(x => x.FullName.Contains(domainName)).ToList().Count} testing problems");
            }

            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "CSMMacros", "Strong", _timeLimit);
            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "CSMMacros", "Weak1m", _timeLimit);
            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "CSMMacros", "Weak5m", _timeLimit);

            //GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PDDLSharpMacros", "Strong", _timeLimit);
            //GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PDDLSharpMacros", "Weak1m", _timeLimit);
            //GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PDDLSharpMacros", "Weak5m", _timeLimit);

            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PredicateMetaActions", "Strong", _timeLimit);
            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PredicateMetaActions", "Weak1m", _timeLimit);
            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PredicateMetaActions", "Weak5m", _timeLimit);

            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PreconditionMetaActions", "Strong", _timeLimit);
            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PreconditionMetaActions", "Weak1m", _timeLimit);
            GenerateScript(domains, sortedTrainProblems, sortedTestProblems, "PreconditionMetaActions", "Weak5m", _timeLimit);
        }

        private static void GenerateScript(List<FileInfo> domains, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, string method, string verification, int timeout)
        {
            var relative = Directory.GetCurrentDirectory();
            var sb = new StringBuilder($"dotnet run --configuration Release --project MetaActions.Train -- \\{Environment.NewLine}");

            sb.AppendLine($"\t--domains\\");
            foreach (var domain in domains)
                sb.AppendLine($"\t\t\t\t\t\t {domain.FullName.Replace(relative, "").Substring(1)} \\");
            sb.AppendLine($"\t--train-problems\\");
            foreach (var problem in trainingProblems)
                sb.AppendLine($"\t\t\t\t\t\t {problem.FullName.Replace(relative, "").Substring(1)} \\");
            sb.AppendLine($"\t--test-problems\\");
            foreach (var problem in testingProblems)
                sb.AppendLine($"\t\t\t\t\t\t {problem.FullName.Replace(relative, "").Substring(1)} \\");

            sb.AppendLine($"\t--generation-strategy {method}\\");
            sb.AppendLine($"\t--verification-strategy {verification}\\");
            sb.AppendLine($"\t--multitask\\");
            sb.AppendLine($"\t--timelimit {timeout}\\");
            sb.AppendLine($"\t--rebuild");

            sb.AppendLine($"cp output/train/*.zip \"MARMA-TestingSets/all_p10_{method}_{verification}_{timeout}m.zip\"");

            File.WriteAllText($"MARMA-TestingSets/all_p16_{method}_{verification}_{timeout}m.sh", sb.ToString());
        }
    }
}