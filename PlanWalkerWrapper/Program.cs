using CommandLine;
using System;
using System.Diagnostics;
using System.Text;
using Tools;
using Tools.Benchmarks;

namespace PlanWalkerWrapper
{
    public class Program : BaseCLI
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<PlanWalkerWrapperOptions>(args)
              .WithParsed(RunPlanWalker)
              .WithNotParsed(HandleParseError);
        }

        public static void RunPlanWalker(PlanWalkerWrapperOptions opts)
        {
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.ProblemFilePath = PathHelper.RootPath(opts.ProblemFilePath);
            opts.MetaDomainPath = PathHelper.RootPath(opts.MetaDomainPath);
            if (opts.OutPlanPath != "")
                opts.OutPlanPath = PathHelper.RootPath(opts.OutPlanPath);
            if (opts.SolutionPath != "")
                opts.SolutionPath = PathHelper.RootPath(opts.SolutionPath);
            opts.WalkerPath = PathHelper.RootPath(opts.WalkerPath);
            opts.FastDownwardPath = PathHelper.RootPath(opts.FastDownwardPath);

            StringBuilder sb = new StringBuilder("run -- ");
            sb.Append($"-d {opts.DomainFilePath} ");
            sb.Append($"-p {opts.ProblemFilePath} ");
            sb.Append($"-m {opts.MetaDomainPath} ");
            sb.Append($"-f {opts.FastDownwardPath} ");
            if (opts.SolutionPath != "")
                sb.Append($"-s {opts.SolutionPath} ");
            if (opts.OutPlanPath != "")
                sb.Append($"-o {opts.OutPlanPath} ");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cargo",
                    Arguments = sb.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = opts.WalkerPath
                }
            };
            process.OutputDataReceived += RecieveOutputData;
            process.ErrorDataReceived += RecieveErrorData;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private static void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"[ERRO] {e.Data}");
        }

        private static void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"[INFO] {e.Data}");
        }

    }
}