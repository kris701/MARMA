using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanSampleGenerator
{
    public interface IPlanFetcher
    {
        public string OutputPath { get; set; }
        public string PythonPrefix { get; set; }
        public string FastDownwardPath { get; set; }
        // Can either be a "--search" or "--alias"
        public string FastDownwardSearch { get; set; }

        public void Fetch(string domain, List<string> problems, bool multithreaded = true);
    }
}
