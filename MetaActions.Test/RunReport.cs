using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Test
{
    public class RunReport
    {
        public string Domain { get; set; }
        public string Problem { get; set; }

        public double SearchTime { get; set; }
        public double TotalTime { get; set; }
        public bool WasSolutionFound { get; set; }

        public Options.ReconstructionMethods ReconstructionMethod { get; set; }

        public RunReport(string domain, string problem, double searchTime, double totalTime, bool wasSolutionFound, Options.ReconstructionMethods reconstructionMethod)
        {
            Domain = domain;
            Problem = problem;
            SearchTime = searchTime;
            TotalTime = totalTime;
            WasSolutionFound = wasSolutionFound;
            ReconstructionMethod = reconstructionMethod;
        }
    }
}
