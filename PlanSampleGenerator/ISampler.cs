using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanSampleGenerator
{
    public interface ISampler
    {
        public List<string> Sample(List<string> source, int count);
    }
}
