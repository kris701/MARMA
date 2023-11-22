using CommandLine;
using MetaActions.Train.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.VerificationStrategies
{
    public abstract class BaseVarificationStrategy : BaseCancelable, IVerificationStrategy
    {
        public enum VerificationResult { None = -1, Success = 0, Failure = 1, TimedOut = 2}
        public List<ValidMetaAction> CurrentlyValidMetaActions { get; }
        internal string _tempCompiledPath = "compiled";
        internal string _tempVerificationPath = "verification";
        internal string _tempExtractedPath = "extracted";
        internal string _tempReplacementsPath = "replacements";
        internal string _tempVerificationReplacementsPath = "replacements";

        public BaseVarificationStrategy(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, token)
        {
            CurrentlyValidMetaActions = new List<ValidMetaAction>();
            _tempCompiledPath = Path.Combine(tempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(tempPath, _tempVerificationPath);
            _tempExtractedPath = Path.Combine(tempPath, _tempExtractedPath);
            _tempReplacementsPath = Path.Combine(tempPath, _tempReplacementsPath);
            _tempVerificationReplacementsPath = Path.Combine(_tempVerificationPath, _tempVerificationReplacementsPath);
            PathHelper.RecratePath(_tempCompiledPath);
            PathHelper.RecratePath(_tempVerificationPath);
            PathHelper.RecratePath(_tempExtractedPath);
            PathHelper.RecratePath(_tempReplacementsPath);
            PathHelper.RecratePath(_tempVerificationReplacementsPath);
        }

        public abstract List<ValidMetaAction> VerifyMetaActions(FileInfo domain, List<FileInfo> allMetaActions, List<FileInfo> verificationProblem);

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

        internal VerificationResult VerifyMetaAction(int timeLimit = -1)
        {
            ArgsCaller stackelVerifier = ArgsCallerBuilder.GetDotnetRunner("StackelbergVerifier");
            _activeProcess = stackelVerifier.Process;
            stackelVerifier.Arguments.Add("--domain", Path.Combine(_tempCompiledPath, "simplified_domain.pddl"));
            stackelVerifier.Arguments.Add("--problem", Path.Combine(_tempCompiledPath, "simplified_problem.pddl"));
            stackelVerifier.Arguments.Add("--output", _tempVerificationPath);
            if (timeLimit != -1 && timeLimit > 0)
                stackelVerifier.Arguments.Add("--time-limit", $"{timeLimit}");
            stackelVerifier.Arguments.Add("--iseasy", "");
            var code = stackelVerifier.Run();
            if (!Enum.GetValues(typeof(VerificationResult)).Cast<int>().Any(x => x == code) && !CancellationToken.IsCancellationRequested)
            {
                Print("Stackelberg Verification failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
            return (VerificationResult)code;
        }

        internal List<FileInfo> ExtractMacrosFromPlans(FileInfo domain, string metaName)
        {
            var replacementsPath = Path.Combine(_tempReplacementsPath, metaName);
            PathHelper.RecratePath(Path.Combine(_tempExtractedPath, metaName));
            PathHelper.RecratePath(replacementsPath);
            IOHelper.CopyFilesRecursively(_tempVerificationReplacementsPath, replacementsPath);
            ArgsCaller macroExtractor = ArgsCallerBuilder.GetDotnetRunner("MacroExtractor");
            _activeProcess = macroExtractor.Process;
            macroExtractor.Arguments.Add("--domain", domain.FullName);
            string macroPlansStr = "";
            var planFiles = new DirectoryInfo(replacementsPath).GetFiles();
            if (planFiles.Count() == 0)
                return new List<FileInfo>();
            foreach (var plan in planFiles)
                macroPlansStr += $" {plan.FullName}";
            macroExtractor.Arguments.Add("--follower-plans", macroPlansStr);
            macroExtractor.Arguments.Add("--output", _tempExtractedPath);
            if (macroExtractor.Run() != 0 && !CancellationToken.IsCancellationRequested)
            {
                Print("Macro Extractor failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
            return new DirectoryInfo(Path.Combine(_tempExtractedPath, metaName)).GetFiles().ToList();
        }
    }
}
