using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.VerificationStrategies
{
    public class WeakUsefulVerificationStrategy : BaseUsefulVerificationStrategy
    {
        public int TimeLimit { get; set; }
        public WeakUsefulVerificationStrategy(string name, int runID, string tempPath, CancellationTokenSource token, int timeLimit) : base(name, runID, tempPath, token)
        {
            TimeLimit = timeLimit;
        }

        public override List<ValidMetaAction> VerifyMetaActions(FileInfo domain, List<FileInfo> allMetaActions, List<FileInfo> verificationProblem)
        {
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                if (CancellationToken.IsCancellationRequested) return CurrentlyValidMetaActions;
                PathHelper.RecratePath(_tempVerificationReplacementsPath);
                Print($"\tTesting meta action {metaActionCounter} of {allMetaActions.Count} [{Math.Round(metaActionCounter / (double)allMetaActions.Count * 100, 0)}%]", ConsoleColor.Magenta);
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in verificationProblem)
                {
                    if (CancellationToken.IsCancellationRequested) return CurrentlyValidMetaActions;
                    Print($"\t\tProblem {problemCounter} out of {verificationProblem.Count} [{Math.Round(problemCounter / (double)verificationProblem.Count * 100, 0)}%].", ConsoleColor.DarkMagenta);
                    // Compile Meta Actions
                    Print($"\t\tCompiling meta action.", ConsoleColor.DarkMagenta);
                    CompileMetaAction(domain.FullName, problem.FullName, metaAction.FullName);

                    // Verify Meta Actions
                    Print($"\t\tVerifying meta action.", ConsoleColor.DarkMagenta);
                    var verificationResult = VerifyMetaAction(TimeLimit);

                    // Stop if invalid
                    if (verificationResult == VerificationResult.None || verificationResult == VerificationResult.Failure)
                    {
                        Print($"\tMeta action was invalid in problem '{problem.Name}'.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    else if (verificationResult == VerificationResult.TimedOut)
                        Print($"\tMeta action timed out on problem '{problem.Name}'. Assuming weak. Continuing...", ConsoleColor.Yellow);
                    problemCounter++;
                }
                if (allValid)
                {
                    if (Directory.Exists(_tempVerificationReplacementsPath) && Directory.GetFiles(_tempVerificationReplacementsPath).Count() > 0)
                    {
                        Print($"\tGenerating initial meta domain...", ConsoleColor.Magenta);
                        GenerateMetaDomain(domain, metaAction, _tempUsefulPath);

                        Print("\tChecking for meta action usefulness...", ConsoleColor.Magenta);
                        if (!IsMetaActionUseful(metaAction, verificationProblem, _tempUsefulPath))
                            continue;
                        Print("\tMeta Action is Useful!", ConsoleColor.Green);

                        Print($"\tExtracting macros from plans...", ConsoleColor.Magenta);
                        CurrentlyValidMetaActions.Add(new ValidMetaAction(metaAction, ExtractMacrosFromPlans(domain, metaAction.Name.Replace(metaAction.Extension, ""))));
                    }
                }
                metaActionCounter++;
            }
            Print($"A total of {CurrentlyValidMetaActions.Count} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green);
            return CurrentlyValidMetaActions;
        }
    }
}
