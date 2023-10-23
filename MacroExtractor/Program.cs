using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.CodeGenerators.Plans;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.Plans;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Parsers.Plans;
using PDDLSharp.Toolkit.MacroGenerators;
using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Tools;

namespace MacroExtractor
{
    internal class Program : BaseCLI
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(ExtractMacros)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        public static void ExtractMacros(Options opts)
        {
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Matching plans...");
            var leaderFollowerPairs = GetLeaderFollowerFilePairs(opts.LeaderPlans.ToList(), opts.FollowerPlans.ToList());
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Extracting reconstruction data...");
            Dictionary<GroundedAction, HashSet<ActionPlan>> macros = new Dictionary<GroundedAction, HashSet<ActionPlan>>();
            IErrorListener listener = new ErrorListener();
            IParser<ActionPlan> parser = new FastDownwardPlanParser(listener);
            foreach (var leaderPlanFile in leaderFollowerPairs.Keys)
            {
                var leaderPlan = parser.Parse(leaderPlanFile);
                int index = IndexOfMetaAction(leaderPlan);
                var metaAction = leaderPlan.Plan[index];
                Dictionary<string, string> nameDictionary = new Dictionary<string, string>();
                int argIndex = 0;
                foreach (var arg in metaAction.Arguments)
                {
                    nameDictionary.Add(arg.Name, $"?{argIndex++}");
                    arg.Name = nameDictionary[arg.Name];
                }

                foreach (var followerPlanFile in leaderFollowerPairs[leaderPlanFile])
                {
                    var followerPlan = parser.Parse(followerPlanFile);
                    var groundedMacroSequence = followerPlan.Plan.GetRange(index, followerPlan.Plan.Count - index);
                    argIndex = 0;
                    foreach (var sequence in groundedMacroSequence)
                        foreach (var arg in sequence.Arguments)
                            arg.Name = nameDictionary[arg.Name];

                    if (macros.ContainsKey(metaAction))
                        macros[metaAction].Add(new ActionPlan(groundedMacroSequence, groundedMacroSequence.Count));
                    else
                        macros.Add(metaAction, new HashSet<ActionPlan>() { new ActionPlan(groundedMacroSequence, groundedMacroSequence.Count) });
                }
            }
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting reconstruction data...");
            OutputReconstructionData(macros, opts.OutputPath);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }

        private static Dictionary<FileInfo, List<FileInfo>> GetLeaderFollowerFilePairs(List<string> leaderPlanFiles, List<string> followerPlanFiles)
        {
            List<FileInfo> leaderPlans = PathHelper.ResolveWildcards(leaderPlanFiles);
            List<FileInfo> followerPlans = PathHelper.ResolveWildcards(followerPlanFiles);

            Dictionary<FileInfo, List<FileInfo>> leaderFollowerPairs = new Dictionary<FileInfo, List<FileInfo>>();
            foreach (var leaderPlan in leaderPlans)
            {
                var leaderName = leaderPlan.Name.Replace(".plan", "");
                leaderFollowerPairs.Add(leaderPlan, new List<FileInfo>());
                foreach (var followerPlan in followerPlans)
                    if (followerPlan.Name.Replace(".plan", "").StartsWith($"{leaderName}_"))
                        leaderFollowerPairs[leaderPlan].Add(followerPlan);
            }
            return leaderFollowerPairs;
        }

        private static int IndexOfMetaAction(ActionPlan leaderPlan)
        {
            int index = 0;
            for (int i = 0; i < leaderPlan.Plan.Count; i++)
            {
                if (leaderPlan.Plan[i].ActionName.StartsWith("$meta"))
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        private static void OutputReconstructionData(Dictionary<GroundedAction, HashSet<ActionPlan>> macros, string outPath)
        {
            IErrorListener listener = new ErrorListener();
            ICodeGenerator<ActionPlan> codeGenerator = new FastDownwardPlanGenerator(listener);
            foreach (var key in macros.Keys)
            {
                PathHelper.RecratePath(Path.Combine(outPath, key.ActionName));
                int id = 1;
                foreach (var replacement in macros[key])
                    codeGenerator.Generate(replacement, Path.Combine(outPath, key.ActionName, $"sequence{id++}.plan"));
            }
        }
    }
}