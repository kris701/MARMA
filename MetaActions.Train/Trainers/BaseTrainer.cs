using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.CodeGenerators;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;
using PDDLSharp.Parsers.FastDownward.Plans;

namespace MetaActions.Train.Trainers
{
    public abstract class BaseTrainer : ITrainer
    {
        internal string _tempProblemPath = "problems";
        internal string _tempMacroPath = "macros";
        internal string _tempMacroTempPath = "macrosTemp";
        internal string _tempMacroGeneratorPath = "macrosGeneratorTemp";
        internal string _tempMetaActionPath = "metaActions";
        internal string _tempCompiledPath = "compiled";
        internal string _tempVerificationPath = "verification";
        internal string _tempReplacementsPath = "replacements";

        internal string _outProblems = "problems";
        internal string _outCache = "cache";

        internal string _cachePath = "cache";
        internal string _macroCachePath = "macros";

        public FileInfo Domain { get; }
        public string DomainName { get; }
        public List<FileInfo> TrainingProblems { get; }
        public List<FileInfo> TestingProblems { get; }
        public TimeSpan TimeLimit { get; } = TimeSpan.FromHours(2);
        public string TempPath { get; }
        public string OutPath { get; }
        public bool OnlyUsefuls { get; }
        public CancellationTokenSource CancellationToken { get; }
        private int _runID = -1;
        public int RunID { get {
                if (_runID != -1)
                    return _runID;
                _runID = GetDeterministicHashCode(Domain.FullName).GetHashCode() + GetDeterministicHashCode(GetType().Name);
                foreach (var trainProblem in TrainingProblems)
                    _runID ^= GetDeterministicHashCode(trainProblem.FullName).GetHashCode();
                return _runID;
            }
        }

        internal List<FileInfo> _currentMetaActions = new List<FileInfo>();
        internal Process? _activeProcess;

        protected BaseTrainer(string domainName, FileInfo domain, List<FileInfo> trainingProblems, List<FileInfo> testingProblems, TimeSpan timeLimit, string tempPath, string outPath, bool usefuls)
        {
            DomainName = domainName;
            Domain = domain;
            TrainingProblems = trainingProblems;
            TestingProblems = testingProblems;
            TimeLimit = timeLimit;
            CancellationToken = new CancellationTokenSource();
            CancellationToken.Token.Register(Kill);
            TempPath = PathHelper.RootPath(tempPath);
            OutPath = PathHelper.RootPath(outPath);
            OnlyUsefuls = usefuls;

            PathHelper.RecratePath(TempPath);
            PathHelper.RecratePath(OutPath);

            _cachePath = PathHelper.RootPath(_cachePath);
            _macroCachePath = Path.Combine(_cachePath, _macroCachePath);

            _tempProblemPath = Path.Combine(TempPath, _tempProblemPath);
            _tempMacroPath = Path.Combine(TempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(TempPath, _tempMacroTempPath);
            _tempMacroGeneratorPath = Path.Combine(TempPath, _tempMacroGeneratorPath);
            _tempMetaActionPath = Path.Combine(TempPath, _tempMetaActionPath);
            _tempCompiledPath = Path.Combine(TempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(TempPath, _tempVerificationPath);
            _tempReplacementsPath = Path.Combine(_tempVerificationPath, _tempReplacementsPath);

            _outProblems = Path.Combine(OutPath, _outProblems);
            _outCache = Path.Combine(OutPath, _outCache);

            PathHelper.RecratePath(_tempProblemPath);
            PathHelper.RecratePath(_tempMacroPath);
            PathHelper.RecratePath(_tempMacroTempPath);
            PathHelper.RecratePath(_tempMacroGeneratorPath);
            PathHelper.RecratePath(_tempMetaActionPath);
            PathHelper.RecratePath(_tempCompiledPath);
            PathHelper.RecratePath(_tempVerificationPath);
            PathHelper.RecratePath(_tempReplacementsPath);

            PathHelper.RecratePath(_outProblems);
            PathHelper.RecratePath(_outCache);

            CopyTestingProblems(TestingProblems, _outProblems);
            GenerateMetaDomain(Domain, _currentMetaActions, OutPath, TempPath);
        }

        public Task<RunReport?> RunTask()
        {
            return new Task<RunReport?>(Run);
        }
        public abstract RunReport? Run();

        public void Kill()
        {
            if (_activeProcess != null)
            {
                try
                {
                    _activeProcess.Kill(true);
                }
                catch { }
            }

            GenerateMetaDomain(Domain, _currentMetaActions, OutPath, TempPath);
        }

        public void Dispose()
        {
            if (CancellationToken != null)
                CancellationToken.Cancel();
            Kill();
        }

        internal void Print(string text, ConsoleColor color)
        {
            ConsoleHelper.WriteLineColor($"\t[{DomainName}] {text}", color);
        }

        private static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        internal List<FileInfo> CopyProblemsToTemp(List<FileInfo> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                if (CancellationToken.IsCancellationRequested)
                    return new List<FileInfo>();
                File.Copy(problem.FullName, Path.Combine(_tempProblemPath, problem.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, problem.Name)));
            }
            return problems;
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

        internal List<FileInfo> GenerateMetaActions()
        {
            ArgsCaller metaCaller = ArgsCallerBuilder.GetDotnetRunner("MetaActionGenerator");
            _activeProcess = metaCaller.Process;
            metaCaller.Arguments.Add("--macros", _tempMacroPath);
            metaCaller.Arguments.Add("--output", _tempMetaActionPath);
            if (metaCaller.Run() != 0 && !CancellationToken.IsCancellationRequested)
            {
                Print("Meta Action Generation failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
            return new DirectoryInfo(_tempMetaActionPath).GetFiles().ToList();
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
