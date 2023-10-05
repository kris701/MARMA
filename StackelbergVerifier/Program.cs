using CommandLine;
using StackelbergVerifier;
using System;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using Tools;

namespace StacklebergVerifier
{
    internal class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<StackelbergVerifierOptions>(args)
              .WithNotParsed(HandleParseError)
              .WithParsed(RunStacklebergVerifier);
        }

        public static void RunStacklebergVerifier(StackelbergVerifierOptions opts)
        {
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            if (!Directory.Exists(opts.OutputPath))
                Directory.CreateDirectory(opts.OutputPath);
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.StackelbergPath = PathHelper.RootPath(opts.StackelbergPath);

            StringBuilder sb = new StringBuilder("");
            sb.Append($"{opts.StackelbergPath} ");
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
                    FileName = opts.PythonPrefix,
                    Arguments = sb.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = opts.OutputPath
                }
            };

            process.Start();
            Console.WriteLine("Verifying...");
            while(!process.HasExited)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine();
            Console.WriteLine("Done!");
            Console.WriteLine("Checking frontier...");
            if (IsFrontierValid(Path.Combine(opts.OutputPath, "pareto_frontier.json")))
                Console.WriteLine("== Frontier is valid ==");
            else
                Console.WriteLine("== Frontier is not valid ==");
        }

        private static bool IsFrontierValid(string file)
        {
            if (!File.Exists(file))
                return false;
            var text = File.ReadAllText(file);
            var index = text.LastIndexOf("\"defender cost\": ") + "\"defender cost\": ".Length;
            var endIndex = text.IndexOf(",", index);
            var numberStr = text.Substring(index, endIndex - index);
            var number = Int32.Parse(numberStr);
            if (number != int.MaxValue)
                return true;
            return false;
        }

    }
}