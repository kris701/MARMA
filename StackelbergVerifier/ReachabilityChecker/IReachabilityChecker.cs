using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackelbergVerifier.ReachabilityChecker
{
    public interface IReachabilityChecker
    {
        public enum ReachabilityResult { None, Possible, Impossible, TimedOut }
        public string TempPath { get; }
        public ReachabilityResult IsTaskPossible(FileInfo domain, FileInfo problem);
    }
}
