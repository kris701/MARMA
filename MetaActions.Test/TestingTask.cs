using PDDLSharp.CodeGenerators;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Test
{
    public class TestingTask : IDisposable
    {
        public FileInfo Domain { get; set; }
        public FileInfo? MetaDomain { get; set; }
        public FileInfo Problem { get; set; }
        public string PlanName { get; set; }
        public string MetaPlanName { get; set; }
        public string SASName { get; set; }
        public int TimeLimit { get; set; }
        public string Alias { get; set; }
        public string CachePath { get; set; }
        public Options.ReconstructionMethods ReconstructionMethod { get; set; }

        private Task? _runningTask;
        private Process? _activeProcess;
        private string _log = "";
        private string _errLog = "";
        private string _fastDownward = PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py");
        private CancellationTokenSource? _tokenSource;

        public TestingTask(int timeLimit, string alias, FileInfo domain, FileInfo? metaDomain, FileInfo problem, string planName, string metaPlan, string sasName, Options.ReconstructionMethods reconstructionMethod, string cachePath)
        {
            TimeLimit = timeLimit;
            Alias = alias;
            ReconstructionMethod = reconstructionMethod;
            Domain = domain;
            MetaDomain = metaDomain;
            Problem = problem;
            PlanName = planName;
            MetaPlanName = metaPlan;
            SASName = sasName;
            CachePath = cachePath;
        }

        public void Kill()
        {
            if (_activeProcess != null)
                _activeProcess.Kill(true);
        }

        public RunReport RunTest(CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource;
            tokenSource.Token.Register(Kill);
            if (Domain.Directory == null || Domain.Directory.Parent == null)
                throw new Exception();
            var domainName = Domain.Directory.Parent.Name;
            if (MetaDomain != null)
                domainName = $"(meta) {domainName}";
            var problemName = Problem.Name.Replace(".pddl", "");
            Program.WriteToConsoleAndLog($"\t[{domainName}, {problemName}] Starting...", ConsoleColor.Magenta);

            RunReport? runReport = null;
            _runningTask = Task.Run(() => {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                if (MetaDomain != null)
                {
                    Program.WriteToConsoleAndLog($"\t[{domainName}, {problemName}] Finding meta plan...", ConsoleColor.Magenta);
                    ExecuteAsNormal(MetaDomain, Problem, MetaPlanName, SASName);
                    if (tokenSource.IsCancellationRequested)
                        return;
                    Program.WriteToConsoleAndLog($"\t[{domainName}, {problemName}] Repairing Plan...", ConsoleColor.Magenta);
                    RepairPlan(Domain, MetaDomain, Problem, PlanName, MetaPlanName);
                    if (tokenSource.IsCancellationRequested)
                        return;
                }
                else
                {
                    Program.WriteToConsoleAndLog($"\t[{domainName}, {problemName}] Finding plan...", ConsoleColor.Magenta);
                    ExecuteAsNormal(Domain, Problem, PlanName, SASName);
                    if (tokenSource.IsCancellationRequested)
                        return;
                }
                timer.Stop();

                runReport = new RunReport(domainName, problemName, GetSearchTimeFromLog(), timer.ElapsedMilliseconds, GetWasSolutionFound(), ReconstructionMethod);
            });
            _runningTask.Wait();
            if (runReport == null)
                return new RunReport(domainName, problemName, 0, 0, false, Options.ReconstructionMethods.None);
            return runReport;
        }

        private double GetSearchTimeFromLog()
        {
            var searchStart = _log.LastIndexOf("INFO     Planner time:") + "INFO     Planner time:".Length;
            var searchEnd = _log.IndexOf("s", searchStart);
            var timeStr = _log.Substring(searchStart, searchEnd - searchStart);
            return double.Parse(timeStr) * 1000;
        }

        private bool GetWasSolutionFound() => _log.Contains("Solution found.");

        private void ExecuteAsNormal(FileInfo domain, FileInfo problem, string planName, string sasName)
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
                fdCaller.Arguments.Add("--overall-time-limit", $"{TimeLimit}m");
                fdCaller.Arguments.Add(domain.FullName, "");
                fdCaller.Arguments.Add(problem.FullName, "");
                if (fdCaller.Run() != 0 && _tokenSource != null && !_tokenSource.IsCancellationRequested)
                {
                    Program.WriteToConsoleAndLog($"Fast Downward Failed!", ConsoleColor.Red);
                    Program.WriteToConsoleAndLog(_errLog, ConsoleColor.Red);
                    _tokenSource.Cancel();
                }
            }
        }

        private void RepairPlan(FileInfo domain, FileInfo metaDomain, FileInfo problem, string planName, string metaPlan)
        {
            using (ArgsCaller reconstructionFixer = ArgsCallerBuilder.GetRustRunner("reconstruction"))
            {
                _activeProcess = reconstructionFixer.Process;
                reconstructionFixer.StdErr += LogStdErr;
                reconstructionFixer.Arguments.Add("-d", domain.FullName);
                reconstructionFixer.Arguments.Add("-p", problem.FullName);
                reconstructionFixer.Arguments.Add("-m", metaDomain.FullName);
                reconstructionFixer.Arguments.Add("-s", metaPlan);
                if (ReconstructionMethod == Options.ReconstructionMethods.MacroCache)
                    reconstructionFixer.Arguments.Add("-c", CachePath);
                reconstructionFixer.Arguments.Add("-f", _fastDownward);
                reconstructionFixer.Arguments.Add("-o", planName);
                if (reconstructionFixer.Run() != 0 && _tokenSource != null && !_tokenSource.IsCancellationRequested)
                {
                    Program.WriteToConsoleAndLog($"Reconstruction Failed!", ConsoleColor.Red);
                    Program.WriteToConsoleAndLog(_errLog, ConsoleColor.Red);
                    _tokenSource.Cancel();
                }
            }
        }

        private void LogStdOut(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _log += $"{e.Data}{Environment.NewLine}";
        }

        private void LogStdErr(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _errLog += $"{e.Data}{Environment.NewLine}";
        }

        public void Dispose()
        {
            if (_tokenSource != null)
                _tokenSource.Cancel();
            Kill();
        }
    }
}
