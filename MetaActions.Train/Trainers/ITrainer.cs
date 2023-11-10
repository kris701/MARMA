using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Train.Trainers
{
    public interface ITrainer : IDisposable
    {
        public FileInfo Domain { get; }
        public string DomainName { get; }
        public List<FileInfo> TrainingProblems { get; }
        public List<FileInfo> TestingProblems { get; }
        public TimeSpan TimeLimit { get; }
        public string TempPath { get; }
        public string OutPath { get; }
        public bool OnlyUsefuls { get; }
        public int RunID { get; }

        public CancellationTokenSource CancellationToken { get; }
        public Task<RunReport?> RunTask();
        public RunReport? Run();
    }
}
