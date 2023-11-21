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
using static MetaActions.Learn.Options;

namespace MetaActions.Train.Trainers
{
    public class MARMATrainer : BaseCancelable, ITrainer
    {
        internal string _tempCompiledPath = "compiled";
        internal string _tempVerificationPath = "verification";
        internal string _tempReplacementsPath = "replacements";

        internal string _outTestProblems = "problems";
        internal string _outCache = "cache";

        public FileInfo Domain { get; }
        public List<FileInfo> TrainingProblems { get; }
        public List<FileInfo> TestingProblems { get; }
        public TimeSpan TimeLimit { get; } = TimeSpan.FromHours(2);
        public string TempPath { get; }
        public string OutPath { get; }
        public bool OnlyUsefuls { get; }
        public IMetaActionStrategy MetaActionStrategy { get; }

        internal List<FileInfo> _currentMetaActions = new List<FileInfo>();
        internal int _totalMetaActions = 0;
        private bool _isDone = false;

        public MARMATrainer(string domainName, FileInfo domain, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, TimeSpan timeLimit, string tempPath, string outPath, bool usefuls, MetaActionStrategy metaActionStrategy) : base(domainName, 1, new CancellationTokenSource())
        {
            var _runID = GetDeterministicHashCode(domain.FullName).GetHashCode() + GetDeterministicHashCode(GetType().Name);
            foreach (var trainProblem in trainingProblems)
                _runID ^= GetDeterministicHashCode(trainProblem.FullName).GetHashCode();
            RunID = _runID;

            switch (metaActionStrategy)
            {
                case Learn.Options.MetaActionStrategy.CSMMacros: MetaActionStrategy = new CSMMacros(domainName, RunID, tempPath, CancellationToken); break;
                case Learn.Options.MetaActionStrategy.PDDLSharpMacros: MetaActionStrategy = new PDDLSharpMacros(domainName, RunID, tempPath, CancellationToken); break;
                default:
                    throw new Exception("Unknown meta action strategy!");
            }

            Domain = domain;
            TrainingProblems = trainingProblems;
            TestingProblems = testingProblems;
            TimeLimit = timeLimit;
            TempPath = PathHelper.RootPath(tempPath);
            OutPath = PathHelper.RootPath(outPath);
            OnlyUsefuls = usefuls;

            PathHelper.RecratePath(TempPath);
            PathHelper.RecratePath(OutPath);

            _tempCompiledPath = Path.Combine(TempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(TempPath, _tempVerificationPath);
            _tempReplacementsPath = Path.Combine(_tempVerificationPath, _tempReplacementsPath);

            _outTestProblems = Path.Combine(OutPath, _outTestProblems);
            _outCache = Path.Combine(OutPath, _outCache);
            PathHelper.RecratePath(_tempCompiledPath);
            PathHelper.RecratePath(_tempVerificationPath);
            PathHelper.RecratePath(_tempReplacementsPath);

            PathHelper.RecratePath(_outTestProblems);
            PathHelper.RecratePath(_outCache);

            CopyTestingProblems(TestingProblems, _outTestProblems);
            GenerateMetaDomain(Domain, _currentMetaActions, OutPath, TempPath);
        }

        public Task<RunReport?> RunTask()
        {
            return new Task<RunReport?>(Run);
        }
        public RunReport? Run()
        {
            _isDone = false;
            Print($"Training started ({GetType().Name})", ConsoleColor.Blue);
            var timer = new System.Timers.Timer();
            timer.Interval = TimeLimit.TotalMilliseconds;
            timer.AutoReset = false;
            timer.Elapsed += (s, e) =>
            {
                CancellationToken.Cancel();
            };
            timer.Start();

            Print($"There is a total of {TrainingProblems.Count} problems to train with.", ConsoleColor.Blue);

            Print($"Getting meta actions...", ConsoleColor.Blue);
            var allMetaActions = MetaActionStrategy.GetMetaActions(Domain, TrainingProblems);
            if (CancellationToken.IsCancellationRequested) return null;

            Print($"Validating meta actions...", ConsoleColor.Blue);
            VerifyMetaActions(allMetaActions);
            if (CancellationToken.IsCancellationRequested) return null;

            if (!CancellationToken.IsCancellationRequested)
                _isDone = true;

            return new RunReport(Name, allMetaActions.Count, _totalMetaActions, _currentMetaActions.Count);
        }

        internal void VerifyMetaActions(List<FileInfo> allMetaActions)
        {
            _totalMetaActions = 0;
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                if (CancellationToken.IsCancellationRequested)
                    return;
                PathHelper.RecratePath(_tempReplacementsPath);
                Print($"\tTesting meta action {metaActionCounter} of {allMetaActions.Count} [{Math.Round(metaActionCounter / (double)allMetaActions.Count * 100, 0)}%]", ConsoleColor.Magenta);
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in TrainingProblems)
                {
                    if (CancellationToken.IsCancellationRequested)
                        return;
                    Print($"\t\tProblem {problemCounter} out of {TrainingProblems.Count} [{Math.Round(problemCounter / (double)TrainingProblems.Count * 100, 0)}%].", ConsoleColor.DarkMagenta);
                    // Compile Meta Actions
                    Print($"\t\tCompiling meta action.", ConsoleColor.DarkMagenta);
                    CompileMetaAction(Domain.FullName, problem.FullName, metaAction.FullName);

                    // Verify Meta Actions
                    Print($"\t\tVerifying meta action.", ConsoleColor.DarkMagenta);
                    var isMetaActionValid = VerifyMetaAction();

                    // Stop if invalid
                    if (!isMetaActionValid)
                    {
                        Print($"\tMeta action was invalid in problem '{problem.Name}'.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    problemCounter++;
                }
                if (allValid)
                {
                    _totalMetaActions++;
                    Print($"\tMeta action was valid in all {TrainingProblems.Count} problems.", ConsoleColor.Green);

                    if (OnlyUsefuls)
                    {
                        Print($"\tGenerating initial meta domain...", ConsoleColor.Magenta);
                        GenerateMetaDomain(Domain, new List<FileInfo>() { metaAction }, OutPath, TempPath);

                        Print("\tChecking for meta action usefulness...", ConsoleColor.Magenta);
                        if (!IsMetaActionUseful(metaAction, TrainingProblems, TempPath))
                            continue;
                        Print("\tMeta Action is Useful!", ConsoleColor.Green);
                    }

                    _currentMetaActions.Add(metaAction);
                    Print($"\tExtracting macros from plans...", ConsoleColor.Magenta);

                    ExtractMacrosFromPlans(Domain, _tempReplacementsPath, _outCache);
                }
                metaActionCounter++;
            }
            Print($"A total of {_currentMetaActions.Count} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green);

            Print($"Generating final meta domain...", ConsoleColor.Blue);
            GenerateMetaDomain(Domain, _currentMetaActions, OutPath, TempPath);
        }

        public new void Kill()
        {
            if (_activeProcess != null)
            {
                try
                {
                    _activeProcess.Kill(true);
                }
                catch { }
            }

            if (!_isDone)
            {
                Print($"Cancelation requested, saving final version of meta domain...", ConsoleColor.Yellow);
                GenerateMetaDomain(Domain, _currentMetaActions, OutPath, TempPath);
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

        internal void CompileMetaAction(string domain, string problem, string metaAction)
        {
            ArgsCaller stackelCompiler = ArgsCallerBuilder.GetDotnetRunner("StackelbergCompiler");
            _activeProcess = stackelCompiler.Process;
            stackelCompiler.Arguments.Add("--domain", domain);
            stackelCompiler.Arguments.Add("--problem", problem);
            stackelCompiler.Arguments.Add("--meta-action", metaAction);
            stackelCompiler.Arguments.Add("--output", _tempCompiledPath);
            if (stackelCompiler.Run() != 0 && !CancellationToken.IsCancellationRequested)
            {
                Print("Stackelberg Compilation failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
        }

        internal bool VerifyMetaAction()
        {
            ArgsCaller stackelVerifier = ArgsCallerBuilder.GetDotnetRunner("StackelbergVerifier");
            _activeProcess = stackelVerifier.Process;
            stackelVerifier.Arguments.Add("--domain", Path.Combine(_tempCompiledPath, "simplified_domain.pddl"));
            stackelVerifier.Arguments.Add("--problem", Path.Combine(_tempCompiledPath, "simplified_problem.pddl"));
            stackelVerifier.Arguments.Add("--output", _tempVerificationPath);
            stackelVerifier.Arguments.Add("--iseasy", "");
            stackelVerifier.Arguments.Add("--stackelberg", PathHelper.RootPath("Dependencies/stackelberg-planner/src/fast-downward.py"));
            var code = stackelVerifier.Run();
            if (code != 0 && code != 1 && !CancellationToken.IsCancellationRequested)
            {
                Print("Stackelberg Verification failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
            return code == 0;
        }

        internal void GenerateMetaDomain(FileInfo domainFile, List<FileInfo> metaActionFiles, string outFolder, string tempFolder)
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
                var metaAction = parser.ParseAs<ActionDecl>(file);
                domain.Actions.Add(metaAction);
            }
            generator.Generate(domain, Path.Combine(outFolder, "metaDomain.pddl"));
            generator.Generate(domain, Path.Combine(tempFolder, "metaDomain.pddl"));
        }

        internal void ExtractMacrosFromPlans(FileInfo domain, string macroPlans, string outFolder)
        {
            ArgsCaller macroExtractor = ArgsCallerBuilder.GetDotnetRunner("MacroExtractor");
            _activeProcess = macroExtractor.Process;
            macroExtractor.Arguments.Add("--domain", domain.FullName);
            string macroPlansStr = "";
            var planFiles = new DirectoryInfo(macroPlans).GetFiles();
            if (planFiles.Count() == 0 && !CancellationToken.IsCancellationRequested)
                throw new Exception("Error, there where no plans made from the stackelberg planner");
            foreach (var plan in planFiles)
                macroPlansStr += $" {plan.FullName}";
            macroExtractor.Arguments.Add("--follower-plans", macroPlansStr);
            macroExtractor.Arguments.Add("--output", outFolder);
            if (macroExtractor.Run() != 0 && !CancellationToken.IsCancellationRequested)
            {
                Print("Macro Extractor failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
        }

        internal bool IsMetaActionUseful(FileInfo metaAction, List<FileInfo> problems, string tempFolder)
        {
            var listener = new ErrorListener();
            var planParser = new FDPlanParser(listener);

            int counter = 1;
            foreach (var problem in problems)
            {
                if (CancellationToken.IsCancellationRequested)
                    return false;
                Print($"\t\tUseful check on problem '{problem.Name}' [{counter++}/{problems.Count}]", ConsoleColor.DarkMagenta);

                using (ArgsCaller fdCaller = new ArgsCaller("python3"))
                {
                    fdCaller.StdOut += (s, o) => { };
                    fdCaller.StdErr += (s, o) => { };
                    fdCaller.Arguments.Add(PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"), "");
                    fdCaller.Arguments.Add("--alias", "lama-first");
                    fdCaller.Arguments.Add("--overall-time-limit", "5m");
                    fdCaller.Arguments.Add("--plan-file", "plan.plan");
                    fdCaller.Arguments.Add("metaDomain.pddl", "");
                    fdCaller.Arguments.Add(problem.FullName, "");
                    fdCaller.Process.StartInfo.WorkingDirectory = tempFolder;
                    if (fdCaller.Run() == 0)
                    {
                        var plan = planParser.Parse(new FileInfo(Path.Combine(tempFolder, "plan.plan")));
                        if (plan.Plan.Any(y => y.ActionName == metaAction.Name.Replace(metaAction.Extension, "")))
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
