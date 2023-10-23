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
    public class TestingTask
    {
        public int TimeLimit { get; set; }
        public string Alias { get; set; }
        public Options.ReconstructionMethods ReconstructionMethod { get; set; }

        private string _log = "";
        private string _fastDownward = PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py");

        public TestingTask(int timeLimit, string alias, Options.ReconstructionMethods reconstructionMethod)
        {
            TimeLimit = timeLimit;
            Alias = alias;
            ReconstructionMethod = reconstructionMethod;
        }

        public RunReport RunTest(FileInfo domain, FileInfo? metaDomain, FileInfo problem, string planName, string metaPlan, string sasName)
        {
            if (domain.Directory == null)
                throw new Exception();
            var domainName = domain.Directory.Name;
            if (metaDomain != null)
                domainName = $"(meta) {domainName}";
            var problemName = problem.Name.Replace(".pddl","");

            Stopwatch timer = new Stopwatch();
            timer.Start();
            if (metaDomain != null)
            {
                ConsoleHelper.WriteLineColor($"\t[{domainName}, {problemName}] Finding meta plan...", ConsoleColor.Magenta);
                ExecuteAsNormal(domain, problem, planName, sasName);
                ConsoleHelper.WriteLineColor($"\t[{domainName}, {problemName}] Repairing Plan...", ConsoleColor.Magenta);
                RepairPlan(domain, metaDomain, problem, planName, metaPlan);
            }
            else
            {
                ConsoleHelper.WriteLineColor($"\t[{domainName}, {problemName}] Finding plan...", ConsoleColor.Magenta);
                ExecuteAsNormal(domain, problem, planName, sasName);
            }
            timer.Stop();

            ConsoleHelper.WriteLineColor($"\t[{domainName}, {problemName}] Task finished!", ConsoleColor.Magenta);
            return new RunReport(domainName, problemName, GetSearchTimeFromLog(), timer.ElapsedMilliseconds, GetWasSolutionFound());
        }

        private double GetSearchTimeFromLog()
        {
            var searchStart = _log.LastIndexOf("INFO     Planner time:") + "INFO     Planner time:".Length;
            var searchEnd = _log.IndexOf("s", searchStart);
            var timeStr = _log.Substring(searchStart, searchEnd - searchStart);
            return double.Parse(timeStr) * 1000;
        }

        private bool GetWasSolutionFound()
        {
            return _log.Contains("Solution found.");
        }

        private void ExecuteAsNormal(FileInfo domain, FileInfo problem, string planName, string sasName)
        {
            ArgsCaller fdCaller = new ArgsCaller("python3");
            fdCaller.StdOut += LogStdOut;
            fdCaller.Arguments.Add(_fastDownward, "");
            fdCaller.Arguments.Add("--alias", Alias);
            fdCaller.Arguments.Add("--plan-file", planName);
            fdCaller.Arguments.Add("--sas-file", sasName);
            fdCaller.Arguments.Add("--overall-time-limit", $"{TimeLimit}m");
            fdCaller.Arguments.Add(domain.FullName, "");
            fdCaller.Arguments.Add(problem.FullName, "");
            if (fdCaller.Run() != 0)
                throw new Exception("Fast Downward failed!");
        }

        private void RepairPlan(FileInfo domain, FileInfo metaDomain, FileInfo problem, string planName, string metaPlan)
        {
            ArgsCaller reconstructionFixer = ArgsCallerBuilder.GetRustRunner("reconstruction");
            reconstructionFixer.Arguments.Add("-d", domain.FullName);
            reconstructionFixer.Arguments.Add("-p", problem.FullName);
            reconstructionFixer.Arguments.Add("-m", metaDomain.FullName);
            reconstructionFixer.Arguments.Add("-s", metaPlan);
            reconstructionFixer.Arguments.Add("-f", _fastDownward);
            reconstructionFixer.Arguments.Add("-o", planName);
            if (reconstructionFixer.Run() != 0)
                throw new Exception("Reconstruction failed!");
        }

        private void LogStdOut(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _log += $"{e.Data}{Environment.NewLine}";
        }
    }
}
