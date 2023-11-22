﻿using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.CodeGenerators;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.FastDownward.Plans;
using PDDLSharp.Parsers.PDDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.VerificationStrategies
{
    public class StrongUsefulVerificationStrategy : BaseUsefulVerificationStrategy
    {
        public StrongUsefulVerificationStrategy(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, tempPath, token)
        {
        }

        public override List<ValidMetaAction> VerifyMetaActions(FileInfo domain, List<FileInfo> allMetaActions, List<FileInfo> verificationProblem)
        {
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                if (CancellationToken.IsCancellationRequested) return CurrentlyValidMetaActions;
                PathHelper.RecratePath(Path.Combine(metaAction.Name, _tempReplacementsPath));
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
                    var verificationResult = VerifyMetaAction();

                    // Stop if invalid
                    if (verificationResult != VerificationResult.Success)
                    {
                        Print($"\tMeta action was invalid in problem '{problem.Name}'.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    problemCounter++;
                }
                if (allValid)
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
                metaActionCounter++;
            }
            Print($"A total of {CurrentlyValidMetaActions.Count} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green);
            return CurrentlyValidMetaActions;
        }
    }
}
