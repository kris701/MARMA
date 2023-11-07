using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MetaActions.Test.Options;
using Tools;

namespace MetaActions.Test.Reconstructors
{
    public class FastDownwardReconstructor : NoReconstructor
    {
        public new Options.ReconstructionMethods Method => ReconstructionMethods.FastDownward;

        public FileInfo MetaDomain { get; set; }
        public string MetaPlanName { get; set; }

        public FastDownwardReconstructor(FileInfo metaDomain, string metaPlanName, FileInfo domain, FileInfo problem, string alias, string planName, string sasName, TimeSpan timeLimit, CancellationTokenSource cancellationToken) : base(domain, problem, alias, planName, sasName, timeLimit, cancellationToken)
        {
            MetaDomain = metaDomain;
            MetaPlanName = metaPlanName;
        }

        public override RunReport? Run()
        {
            Program.WriteToConsoleAndLog($"\t[{DomainName}, {ProblemName}] Starting...", ConsoleColor.Magenta);
            Program.WriteToConsoleAndLog($"\t[{DomainName}, {ProblemName}] Finding meta plan...", ConsoleColor.Magenta);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            ExecuteAsNormal(Domain, Problem, MetaPlanName, SasName);
            Program.WriteToConsoleAndLog($"\t[{DomainName}, {ProblemName}] Repairing Plan...", ConsoleColor.Magenta);
            RepairPlan(Domain, MetaDomain, Problem, PlanName, MetaPlanName);
            timer.Stop();
            if (CancellationToken.IsCancellationRequested)
                return null;
            return new RunReport(DomainName, ProblemName, GetSearchTimeFromLog(), timer.ElapsedMilliseconds, GetWasSolutionFound(), Method);
        }

        internal virtual void RepairPlan(FileInfo domain, FileInfo metaDomain, FileInfo problem, string planName, string metaPlan)
        {
            using (ArgsCaller reconstructionFixer = ArgsCallerBuilder.GetRustRunner("reconstruction"))
            {
                _activeProcess = reconstructionFixer.Process;
                reconstructionFixer.StdErr += LogStdErr;
                reconstructionFixer.Arguments.Add("-d", domain.FullName);
                reconstructionFixer.Arguments.Add("-p", problem.FullName);
                reconstructionFixer.Arguments.Add("-m", metaDomain.FullName);
                reconstructionFixer.Arguments.Add("-s", metaPlan);
                reconstructionFixer.Arguments.Add("-f", _fastDownward);
                reconstructionFixer.Arguments.Add("-o", planName);
                if (reconstructionFixer.Run() != 0 && !CancellationToken.IsCancellationRequested)
                {
                    Program.WriteToConsoleAndLog($"Reconstruction Failed!", ConsoleColor.Red);
                    Program.WriteToConsoleAndLog(_errLog, ConsoleColor.Red);
                    CancellationToken.Cancel();
                }
            }
        }
    }
}
