using CommandLine;
using StackelbergVerifier;
using StackelbergVerifier.ReachabilityChecker;
using System.Diagnostics;
using System.Text;
using Tools;
using static StackelbergVerifier.ReachabilityChecker.IReachabilityChecker;

namespace StacklebergVerifier
{
    internal class Program : BaseCLI
    {
        private static string _stackelbergPath = PathHelper.RootPath("Dependencies/stackelberg-planner/src/fast-downward.py");
        private static int _returnCode = int.MaxValue;
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<StackelbergVerifierOptions>(args)
              .WithNotParsed(HandleParseError)
              .WithParsed(RunStacklebergVerifier);
            return _returnCode;
        }

        public static void RunStacklebergVerifier(StackelbergVerifierOptions opts)
        {
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);

            ConsoleHelper.WriteLineColor("Verifying paths...");
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);
            if (!File.Exists(opts.DomainFilePath))
                throw new FileNotFoundException($"Domain file not found: {opts.DomainFilePath}");
            if (!File.Exists(opts.ProblemFilePath))
                throw new FileNotFoundException($"Problem file not found: {opts.ProblemFilePath}");
            if (!File.Exists(_stackelbergPath))
                throw new FileNotFoundException($"Stackelberg planner file not found: {_stackelbergPath}");
            if (File.Exists(Path.Combine(opts.OutputPath, "pareto_frontier.json")))
                File.Delete(Path.Combine(opts.OutputPath, "pareto_frontier.json"));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Checking reachability...");
            var checker = new FDReachabilityChecker(Path.Combine(opts.OutputPath, "reachability"));
            var result = checker.IsTaskPossible(new FileInfo(opts.DomainFilePath), new FileInfo(opts.ProblemFilePath));
            if (result == ReachabilityResult.Impossible)
            {
                ConsoleHelper.WriteLineColor("Task impossible to solve!", ConsoleColor.Red);
                _returnCode = 1;
                return;
            }

            ConsoleHelper.WriteLineColor("Executing Stackelberg Planner");
            ConsoleHelper.WriteLineColor("(Note, this may take a while)");
            var process = ExecutePlanner(opts);
            while (!process.HasExited)
            {
                ConsoleHelper.WriteColor(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine();
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            if (process.ExitCode != 0)
            {
                _returnCode = 1;
                ConsoleHelper.WriteLineColor("== Frontier is not valid ==", ConsoleColor.Red);
                return;
            }

            ConsoleHelper.WriteLineColor("Checking Frontier...");
            if (IsFrontierValid(Path.Combine(opts.OutputPath, "pareto_frontier.json")))
            {
                _returnCode = 0;
                ConsoleHelper.WriteLineColor("== Frontier is valid ==", ConsoleColor.Green);
            }
            else
            {
                _returnCode = 1;
                ConsoleHelper.WriteLineColor("== Frontier is not valid ==", ConsoleColor.Red);
            }
        }

        private static bool IsFrontierValid(string file)
        {
            if (!File.Exists(file))
                return false;
            var text = File.ReadAllText(file);
            var index = text.LastIndexOf("\"attacker cost\": ") + "\"attacker cost\": ".Length;
            var endIndex = text.IndexOf(",", index);
            var numberStr = text.Substring(index, endIndex - index);
            var number = int.Parse(numberStr);
            if (number != int.MaxValue)
                return true;
            return false;
        }

        private static Process ExecutePlanner(StackelbergVerifierOptions opts)
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append($"{_stackelbergPath} ");
            sb.Append($"\"{opts.DomainFilePath}\" ");
            sb.Append($"\"{opts.ProblemFilePath}\" ");
            if (opts.IsEasyProblem)
                sb.Append($"--search \"sym_stackelberg(optimal_engine=symbolic(plan_reuse_minimal_task_upper_bound=false, plan_reuse_upper_bound=true), upper_bound_pruning=false)\" ");
            else
                sb.Append($"--search \"sym_stackelberg(optimal_engine=symbolic(plan_reuse_minimal_task_upper_bound=true, plan_reuse_upper_bound=true, force_bw_search_minimum_task_seconds=30, time_limit_seconds_minimum_task=300), upper_bound_pruning=true)\" ");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "python2",
                    Arguments = sb.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = opts.OutputPath
                }
            };

            process.Start();
            return process;
        }

    }
}