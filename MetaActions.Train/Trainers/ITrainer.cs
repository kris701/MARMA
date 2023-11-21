using MetaActions.Train.MetaActionStrategies;

namespace MetaActions.Train.Trainers
{
    public interface ITrainer
    {
        public FileInfo Domain { get; }
        public List<FileInfo> TrainingProblems { get; }
        public List<FileInfo> TestingProblems { get; }
        public TimeSpan TimeLimit { get; }
        public string TempPath { get; }
        public string OutPath { get; }
        public bool OnlyUsefuls { get; }
        public int RunID { get; }
        public IMetaActionStrategy MetaActionStrategy { get; }

        public CancellationTokenSource CancellationToken { get; }
        public Task<RunReport?> RunTask();
        public RunReport? Run();
    }
}
