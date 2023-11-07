using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using static MetaActions.Test.Options;

namespace MetaActions.Test.Reconstructors
{
    public class NoReconstructor : IReconstructor
    {
        public Options.ReconstructionMethods Method => ReconstructionMethods.None;

        public string DomainName { get {
                if (Domain.Directory == null || Domain.Directory.Parent == null)
                    throw new Exception();
                return Domain.Directory.Parent.Name;
            } 
        }
        public string ProblemName => Problem.Name.Replace(".pddl", "");

        public FileInfo Domain { get; }
        public FileInfo Problem { get; }
        public string Alias { get; }
        public string PlanName { get; }
        public string SasName { get; }
        public TimeSpan TimeLimit { get; }
        public CancellationTokenSource CancellationToken { get; }

        internal Process? _activeProcess;
        internal string _log = "";
        internal string _errLog = "";
        internal string _fastDownward = PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py");

        public NoReconstructor(FileInfo domain, FileInfo problem, string alias, string planName, string sasName, TimeSpan timeLimit, CancellationTokenSource cancellationToken)
        {
            Domain = domain;
            Problem = problem;
            Alias = alias;
            PlanName = planName;
            SasName = sasName;
            TimeLimit = timeLimit;
            CancellationToken = cancellationToken;
            CancellationToken.Token.Register(Kill);
        }

        public Task<RunReport?> RunTask()
        {
            return new Task<RunReport?>(Run);
        }

        public virtual RunReport? Run()
        {
            Program.WriteToConsoleAndLog($"\t[{DomainName}, {ProblemName}] Starting...", ConsoleColor.Magenta);
            Program.WriteToConsoleAndLog($"\t[{DomainName}, {ProblemName}] Finding plan...", ConsoleColor.Magenta);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            ExecuteAsNormal(Domain, Problem, PlanName, SasName);
            timer.Stop();
            if (CancellationToken.IsCancellationRequested)
                return null;
            return new RunReport(DomainName, ProblemName, GetSearchTimeFromLog(), timer.ElapsedMilliseconds, GetWasSolutionFound(), Method);
        }

        internal void ExecuteAsNormal(FileInfo domain, FileInfo problem, string planName, string sasName)
        {
            using (ArgsCaller fdCaller = new ArgsCaller("python3"))
            {
                _activeProcess = fdCaller.Process;
                fdCaller.StdOut += LogStdOut;
                fdCaller.StdErr += LogStdErr;
                fdCaller.Arguments.Add(_fastDownward, "");
                fdCaller.Arguments.Add("--alias", Alias);
                fdCaller.Arguments.Add("--plan-file", planName);
                fdCaller.Arguments.Add("--sas-file", sasName);
                fdCaller.Arguments.Add("--overall-time-limit", $"{TimeLimit.TotalMinutes}m");
                fdCaller.Arguments.Add(domain.FullName, "");
                fdCaller.Arguments.Add(problem.FullName, "");
                if (fdCaller.Run() != 0 && !CancellationToken.IsCancellationRequested)
                {
                    Program.WriteToConsoleAndLog($"Fast Downward Failed!", ConsoleColor.Red);
                    Program.WriteToConsoleAndLog(_errLog, ConsoleColor.Red);
                    CancellationToken.Cancel();
                }
            }
        }

        internal bool GetWasSolutionFound() => _log.Contains("Solution found.");
        internal double GetSearchTimeFromLog()
        {
            var searchStart = _log.LastIndexOf("INFO     Planner time:") + "INFO     Planner time:".Length;
            var searchEnd = _log.IndexOf("s", searchStart);
            var timeStr = _log.Substring(searchStart, searchEnd - searchStart);
            return double.Parse(timeStr) * 1000;
        }

        internal void LogStdOut(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _log += $"{e.Data}{Environment.NewLine}";
        }

        internal void LogStdErr(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _errLog += $"{e.Data}{Environment.NewLine}";
        }

        public void Kill()
        {
            if (_activeProcess != null)
            {
                try
                {
                    _activeProcess.Kill(true);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            if (CancellationToken != null)
                CancellationToken.Cancel();
            Kill();
        }
    }
}
