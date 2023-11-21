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
        public List<ValidMetaAction> CurrentlyValidMetaActions { get; }
        internal string _tempCompiledPath = "compiled";
        internal string _tempVerificationPath = "verification";
        internal string _tempExtractedPath = "extracted";
        internal string _tempReplacementsPath = "replacements";

        public BaseVarificationStrategy(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, token)
        {
            CurrentlyValidMetaActions = new List<ValidMetaAction>();
            _tempCompiledPath = Path.Combine(tempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(tempPath, _tempVerificationPath);
            _tempExtractedPath = Path.Combine(tempPath, _tempExtractedPath);
            _tempReplacementsPath = Path.Combine(_tempVerificationPath, _tempReplacementsPath);
            PathHelper.RecratePath(_tempCompiledPath);
            PathHelper.RecratePath(_tempVerificationPath);
            PathHelper.RecratePath(_tempExtractedPath);
            PathHelper.RecratePath(_tempReplacementsPath);
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

        internal bool VerifyMetaAction()
        {
            ArgsCaller stackelVerifier = ArgsCallerBuilder.GetDotnetRunner("StackelbergVerifier");
            _activeProcess = stackelVerifier.Process;
            stackelVerifier.Arguments.Add("--domain", Path.Combine(_tempCompiledPath, "simplified_domain.pddl"));
            stackelVerifier.Arguments.Add("--problem", Path.Combine(_tempCompiledPath, "simplified_problem.pddl"));
            stackelVerifier.Arguments.Add("--output", _tempVerificationPath);
            stackelVerifier.Arguments.Add("--iseasy", "");
            var code = stackelVerifier.Run();
            if (code != 0 && code != 1 && !CancellationToken.IsCancellationRequested)
            {
                Print("Stackelberg Verification failed!", ConsoleColor.Red);
                CancellationToken.Cancel();
            }
            return code == 0;
        }

        internal List<FileInfo> ExtractMacrosFromPlans(FileInfo domain, string metaName)
        {
            ArgsCaller macroExtractor = ArgsCallerBuilder.GetDotnetRunner("MacroExtractor");
            _activeProcess = macroExtractor.Process;
            macroExtractor.Arguments.Add("--domain", domain.FullName);
            string macroPlansStr = "";
            var planFiles = new DirectoryInfo(_tempReplacementsPath).GetFiles();
            if (planFiles.Count() == 0 && !CancellationToken.IsCancellationRequested)
                throw new Exception("Error, there where no plans made from the stackelberg planner");
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
