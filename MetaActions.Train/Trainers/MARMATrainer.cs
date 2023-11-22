using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.CodeGenerators;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Parsers.FastDownward.Plans;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using MetaActions.Train.MetaActionStrategies;
using static MetaActions.Options;
using MetaActions.Train.VerificationStrategies;
using MetaActions.Train.Tools;

namespace MetaActions.Train.Trainers
{
    public class MARMATrainer : BaseCancelable, ITrainer
    {
        internal string _outTestProblems = "problems";
        internal string _outCache = "cache";

        public FileInfo Domain { get; }
        public List<FileInfo> TrainingProblems { get; }
        public List<FileInfo> TestingProblems { get; }
        public TimeSpan TimeLimit { get; } = TimeSpan.FromHours(2);
        public string TempPath { get; }
        public string OutPath { get; }
        public IMetaActionStrategy MetaActionStrategy { get; }
        public IVerificationStrategy MetaActionVerificationStrategy { get; }

        private bool _isDone = false;

        public MARMATrainer(string domainName, FileInfo domain, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, TimeSpan timeLimit, string tempPath, string outPath, MetaActionVerificationStrategy metaActionVerificationStrategy, MetaActionGenerationStrategy metaActionStrategy) : base(domainName, 1, new CancellationTokenSource())
        {
            var _runID = GetDeterministicHashCode(domain.FullName).GetHashCode() + GetDeterministicHashCode($"{Enum.GetName(typeof(MetaActionGenerationStrategy), metaActionVerificationStrategy)}");
            foreach (var trainProblem in trainingProblems)
                _runID ^= GetDeterministicHashCode(trainProblem.FullName).GetHashCode();
            RunID = _runID;

            Domain = domain;
            TrainingProblems = trainingProblems;
            TestingProblems = testingProblems;
            TimeLimit = timeLimit;
            TempPath = PathHelper.RootPath(tempPath);
            OutPath = PathHelper.RootPath(outPath);

            PathHelper.RecratePath(TempPath);
            PathHelper.RecratePath(OutPath);

            _outTestProblems = Path.Combine(OutPath, _outTestProblems);
            _outCache = Path.Combine(OutPath, _outCache);

            PathHelper.RecratePath(_outTestProblems);
            PathHelper.RecratePath(_outCache);

            switch (metaActionStrategy)
            {
                case MetaActionGenerationStrategy.CSMMacros: MetaActionStrategy = new CSMMacros(domainName, RunID, tempPath, CancellationToken); break;
                case MetaActionGenerationStrategy.PDDLSharpMacros: MetaActionStrategy = new PDDLSharpMacros(domainName, RunID, tempPath, CancellationToken); break;
                default:
                    throw new Exception("Unknown meta action generation strategy!");
            }

            switch (metaActionVerificationStrategy)
            {
                case Options.MetaActionVerificationStrategy.Strong: MetaActionVerificationStrategy = new StrongVerificationStrategy(domainName, RunID, tempPath, CancellationToken); break;
                case Options.MetaActionVerificationStrategy.StrongUseful: MetaActionVerificationStrategy = new StrongUsefulVerificationStrategy(domainName, RunID, tempPath, CancellationToken); break;
                case Options.MetaActionVerificationStrategy.Weak1m: MetaActionVerificationStrategy = new WeakVerificationStrategy(domainName, RunID, tempPath, CancellationToken, 1); break;
                case Options.MetaActionVerificationStrategy.Weak1mUseful: MetaActionVerificationStrategy = new WeakUsefulVerificationStrategy(domainName, RunID, tempPath, CancellationToken, 1); break;
                default:
                    throw new Exception("Unknown meta action verification strategy!");
            }

            CopyTestingProblems(TestingProblems, _outTestProblems);
            GenerateMetaDomain(Domain, MetaActionVerificationStrategy.CurrentlyValidMetaActions, OutPath, TempPath);
        }

        public Task<RunReport?> RunTask()
        {
            return new Task<RunReport?>(Run);
        }
        public RunReport? Run()
        {
            _isDone = false;
            Print($"Training started ({GetType().Name})", ConsoleColor.Blue);
            StartTimeoutTimer();

            Print($"There is a total of {TrainingProblems.Count} problems to train with.", ConsoleColor.Blue);

            Print($"Getting meta actions...", ConsoleColor.Blue);
            var allMetaActions = MetaActionStrategy.GetMetaActions(Domain, TrainingProblems);
            if (CancellationToken.IsCancellationRequested) return null;

            Print($"Validating meta actions...", ConsoleColor.Blue);
            var verifiedMetaActions = MetaActionVerificationStrategy.VerifyMetaActions(Domain, allMetaActions, TrainingProblems);
            foreach(var valid in verifiedMetaActions)
            {
                var target = Path.Combine(_outCache, valid.MetaAction.Name.Replace(valid.MetaAction.Extension, ""));
                PathHelper.RecratePath(target);
                foreach (var replacement in valid.Replacements)
                    File.Copy(replacement.FullName, Path.Combine(target, replacement.Name));
            }
            Print($"Generating final meta domain...", ConsoleColor.Blue);
            GenerateMetaDomain(Domain, MetaActionVerificationStrategy.CurrentlyValidMetaActions, OutPath, TempPath);
            if (CancellationToken.IsCancellationRequested) return null;

            if (!CancellationToken.IsCancellationRequested)
                _isDone = true;

            return new RunReport(Name, allMetaActions.Count, MetaActionVerificationStrategy.CurrentlyValidMetaActions.Count);
        }

        private void StartTimeoutTimer()
        {
            var cancelationTimer = new System.Timers.Timer();
            cancelationTimer.Interval = TimeLimit.TotalMilliseconds;
            cancelationTimer.AutoReset = false;
            cancelationTimer.Elapsed += (s, e) =>
            {
                CancellationToken.Cancel();
            };
            cancelationTimer.Start();
        }

        public new void Kill()
        {
            base.Kill();

            if (!_isDone)
            {
                Print($"Cancelation requested, saving final version of meta domain...", ConsoleColor.Yellow);
                GenerateMetaDomain(Domain, MetaActionVerificationStrategy.CurrentlyValidMetaActions, OutPath, TempPath);
            }
        }

        private static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = (hash1 << 5) + hash1 ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = (hash2 << 5) + hash2 ^ str[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }

        internal void CopyTestingProblems(List<FileInfo> testingProblems, string outFolder)
        {
            int id = 0;
            foreach (var problem in testingProblems)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;
                var target = new FileInfo(Path.Combine(outFolder, $"p{id++}.pddl"));
                File.Copy(problem.FullName, target.FullName);
            }
        }

        internal void GenerateMetaDomain(FileInfo domainFile, List<ValidMetaAction> metaActionFiles, string outFolder, string tempFolder)
        {
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);
            ICodeGenerator<INode> generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            var domain = parser.ParseAs<DomainDecl>(domainFile);
            generator.Generate(domain, Path.Combine(outFolder, "domain.pddl"));

            foreach (var file in metaActionFiles)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;
                var metaAction = parser.ParseAs<ActionDecl>(file.MetaAction);
                domain.Actions.Add(metaAction);
            }
            generator.Generate(domain, Path.Combine(outFolder, "metaDomain.pddl"));
            generator.Generate(domain, Path.Combine(tempFolder, "metaDomain.pddl"));
        }
    }
}
