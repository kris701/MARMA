using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using static StackelbergVerifier.ReachabilityChecker.IReachabilityChecker;

namespace StackelbergVerifier.ReachabilityChecker
{
    public class FDReachabilityChecker : IReachabilityChecker
    {
        public string TempPath { get; }

        private static string _fastDownwardPath = PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py");
        private string _stdLog = "";

        public FDReachabilityChecker(string tempPath)
        {
            TempPath = tempPath;
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        public ReachabilityResult IsTaskPossible(FileInfo domain, FileInfo problem)
        {
            _stdLog = "";
            var fdRunner = ArgsCallerBuilder.GetGenericRunner("python3");
            fdRunner.StdOut += (s, e) => { if (e.Data != null) _stdLog += e.Data; };
            fdRunner.Process.StartInfo.WorkingDirectory = TempPath;
            fdRunner.Arguments.Add(_fastDownwardPath, "");
            fdRunner.Arguments.Add($"{domain.FullName}", "");
            fdRunner.Arguments.Add($"{problem.FullName}", "");
            fdRunner.Arguments.Add("--search", "\"eager_greedy([hmax()])\"  ");
            if (fdRunner.Run() == 0)
            {
                if (_stdLog.Contains("Solution found."))
                    return ReachabilityResult.Possible;
                if (_stdLog.Contains("Search stopped without finding a solution."))
                    return ReachabilityResult.Impossible;
            }
            return ReachabilityResult.None;
        }
    }
}
