using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using static MetaActions.Test.Options;

namespace MetaActions.Test.Reconstructors
{
    public class CacheReconstructor : FastDownwardReconstructor
    {
        public new Options.ReconstructionMethods Method => ReconstructionMethods.MacroCache;
        public string CachePath { get; }

        public CacheReconstructor(string cachePath, FileInfo metaDomain, string metaPlanName, FileInfo domain, FileInfo problem, string alias, string planName, string sasName, TimeSpan timeLimit, CancellationTokenSource cancellationToken) : base(metaDomain, metaPlanName, domain, problem, alias, planName, sasName, timeLimit, cancellationToken)
        {
            CachePath = cachePath;
        }

        internal override void RepairPlan(FileInfo domain, FileInfo metaDomain, FileInfo problem, string planName, string metaPlan)
        {
            using (ArgsCaller reconstructionFixer = ArgsCallerBuilder.GetRustRunner("reconstruction"))
            {
                _activeProcess = reconstructionFixer.Process;
                reconstructionFixer.StdErr += LogStdErr;
                reconstructionFixer.Arguments.Add("-d", domain.FullName);
                reconstructionFixer.Arguments.Add("-p", problem.FullName);
                reconstructionFixer.Arguments.Add("-m", metaDomain.FullName);
                reconstructionFixer.Arguments.Add("-s", metaPlan);
                reconstructionFixer.Arguments.Add("-c", CachePath);
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
