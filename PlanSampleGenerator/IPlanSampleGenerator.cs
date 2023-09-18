using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanSampleGenerator
{
    public interface IPlanSampleGenerator
    {
        public string DomainPath { get; set; }
        public List<string> ProblemPaths { get; set; }
        public int SampleCount { get; set; }
        public string OutputPath { get; set; }
        public string FastDownwardPath { get; set; }
        public string FastDownwardArguments { get; set; }

        // If seed is -1, its just conventional random
        public void Sample(int seed = -1);
    }
}
