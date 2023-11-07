using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Test.Reconstructors
{
    public interface IReconstructor : IDisposable
    {
        public Options.ReconstructionMethods Method { get; }

        public string DomainName { get; }
        public string ProblemName { get; }

        public string Alias { get; }
        public TimeSpan TimeLimit { get; }

        public CancellationTokenSource CancellationToken { get; }
        public Task<RunReport?> RunTask();
        public RunReport? Run();
    }
}
