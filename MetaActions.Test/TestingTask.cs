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
        public RunReport RunTest(string domain, string metaDomain, string problem, string planName, string metaPlan, string sasName)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            if (metaDomain != "")
            {
                ExecuteAsMeta(domain, metaDomain, problem, planName, metaPlan, sasName);
            }
            else
            {
                ExecuteAsNormal(domain, problem, planName, sasName);
            }
            timer.Stop();

            //var searchStart = _log.LastIndexOf("] Search time: ") + "] Search time: ".Length;
            //var searchEnd = _log.IndexOf("s", searchStart);
            //var timeStr = _log.Substring(searchStart, searchEnd - searchStart);
            //double searchTime = double.Parse(timeStr);
            var searchTime = 0;

            if (metaDomain != "")
                return new RunReport(metaDomain, problem, searchTime, timer.ElapsedMilliseconds);
            else
                return new RunReport(domain, problem, searchTime, timer.ElapsedMilliseconds);
        }

        private static string _log = "";
        private static void ExecuteAsNormal(string domain, string problem, string planName, string sasName)
        {
            ArgsCaller fdCaller = new ArgsCaller("python3");
            fdCaller.StdOut += LogStdOut;
            fdCaller.StdErr += StopOnError;
            fdCaller.Arguments.Add(PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"), "");
            fdCaller.Arguments.Add("--alias", "lama-first");
            fdCaller.Arguments.Add("--plan-file", planName);
            fdCaller.Arguments.Add("--sas-file", sasName);
            fdCaller.Arguments.Add(domain, "");
            fdCaller.Arguments.Add(problem, "");
            fdCaller.Run();
        }

        private static void LogStdOut(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _log += $"{e.Data}{Environment.NewLine}";
        }

        private static void StopOnError(object sender, DataReceivedEventArgs e)
        {
            //if (e.Data != null)
            //    throw new Exception($"An error occured: {e.Data}");
        }

        private static void ExecuteAsMeta(string domain, string metaDomain, string problem, string planName, string metaPlan, string sasName)
        {
            // Execute with FD
            ExecuteAsNormal(metaDomain, problem, metaPlan, sasName);

            // Reconstruct plan
            ArgsCaller reconstructionFixer = ArgsCallerBuilder.GetRustRunner("reconstruction");
            reconstructionFixer.Arguments.Add("-d", domain);
            reconstructionFixer.Arguments.Add("-p", problem);
            reconstructionFixer.Arguments.Add("-m", metaDomain);
            reconstructionFixer.Arguments.Add("-s", metaPlan);
            reconstructionFixer.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
            reconstructionFixer.Arguments.Add("-o", planName);
            if (reconstructionFixer.Run() != 0)
                throw new Exception("Reconstruction failed!");
        }
    }
}
