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
            List<FileInfo> leaderPlans = PathHelper.ResolveWildcards(opts.LeaderPlans.ToList());
            List<FileInfo> followerPlans = PathHelper.ResolveWildcards(opts.FollowerPlans.ToList());

            Dictionary<FileInfo, List<FileInfo>> leaderFollowerPairs = new Dictionary<FileInfo, List<FileInfo>>();
            foreach(var leaderPlan in leaderPlans)
            {
                var leaderName = leaderPlan.Name.Replace(".plan","");
                leaderFollowerPairs.Add(leaderPlan, new List<FileInfo>());
                foreach (var followerPlan in followerPlans)
                    if (followerPlan.Name.Replace(".plan", "").StartsWith($"{leaderName}_"))
                        leaderFollowerPairs[leaderPlan].Add(followerPlan);
            }
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Extracting reconstruction data...");
            Dictionary<GroundedAction, List<List<GroundedAction>>> macros = new Dictionary<GroundedAction, List<List<GroundedAction>>>();
            IErrorListener listener = new ErrorListener();
            IParser<ActionPlan> parser = new FastDownwardPlanParser(listener);
            foreach (var leaderPlanFile in leaderFollowerPairs.Keys)
            {
                var leaderPlan = parser.Parse(leaderPlanFile);
                int index = IndexOfMetaAction(leaderPlan);
                var metaAction = leaderPlan.Plan[index];
                foreach (var arg in metaAction.Arguments)
                    arg.Name = $"?{arg}";
                foreach (var followerPlanFile in leaderFollowerPairs[leaderPlanFile])
                {
                    var followerPlan = parser.Parse(followerPlanFile);
                    var groundedMacroSequence = followerPlan.Plan.GetRange(index, followerPlan.Plan.Count - index);
                    foreach (var groundedSequence in groundedMacroSequence)
                        foreach (var arg in groundedSequence.Arguments)
                            arg.Name = $"?{arg}";

                    if (macros.ContainsKey(metaAction))
                        macros[metaAction].Add(groundedMacroSequence);
                    else
                        macros.Add(metaAction, new List<List<GroundedAction>>() { groundedMacroSequence });
                }
            }
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting reconstruction data...");
            ICodeGenerator<ActionPlan> codeGenerator = new FastDownwardPlanGenerator(listener);
            foreach(var key in  macros.Keys)
            {
                PathHelper.RecratePath(Path.Combine(opts.OutputPath, key.ActionName));
                int id = 1;
                foreach (var replacement in macros[key])
                    codeGenerator.Generate(new ActionPlan(replacement, replacement.Count), Path.Combine(opts.OutputPath, key.ActionName, $"sequence{id++}.plan"));
            }
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
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
    }
}