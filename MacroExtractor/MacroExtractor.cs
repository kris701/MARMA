using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.FastDownward.Plans;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Parsers.FastDownward.Plans;
using PDDLSharp.Parsers;
using PDDLSharp.Toolkit.MacroGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MacroExtractor
{
    public class MacroExtractor
    {
        public static string _metaActionName = "$meta";
        public static string _macroActionName = "$macro";
        public static string[] _RemoveNamesFromActions = { "attack_", "fix_" };

        public List<RepairSequence> ExtractMacros(DomainDecl domain, List<string> planFiles)
        {
            var planSequences = ExtractUniquePlanSequences(planFiles);
            var macros = GenerateMacros(planSequences, domain);
            return macros.ToList();
        }

        private static Dictionary<GroundedAction, HashSet<ActionPlan>> ExtractUniquePlanSequences(List<string> followerPlanFiles)
        {
            var followerPlans = PathHelper.ResolveFileWildcards(followerPlanFiles);

            var planSequences = new Dictionary<GroundedAction, HashSet<ActionPlan>>();
            IErrorListener listener = new ErrorListener();
            IParser<ActionPlan> parser = new FDPlanParser(listener);

            foreach (var planFile in followerPlans)
            {
                var plan = parser.Parse(planFile);
                int metaActionIndex = IndexOfMetaAction(plan);
                var metaAction = plan.Plan[metaActionIndex];
                var nameDictionary = GenerateNameReplacementDict(metaAction);
                RenameActionArguments(metaAction, nameDictionary);
                if (!planSequences.ContainsKey(metaAction))
                    planSequences.Add(metaAction, new HashSet<ActionPlan>());

                var repairSequence = plan.Plan.GetRange(metaActionIndex + 1, plan.Plan.Count - metaActionIndex - 1);
                foreach (var action in repairSequence)
                    RenameActionArguments(action, nameDictionary);

                planSequences[metaAction].Add(new ActionPlan(repairSequence, repairSequence.Count));
            }

            return planSequences;
        }

        private static Dictionary<string, string> GenerateNameReplacementDict(GroundedAction metaAction)
        {
            var returnDict = new Dictionary<string, string>();
            int argIndex = 0;
            foreach (var arg in metaAction.Arguments)
                if (!returnDict.ContainsKey(arg.Name))
                    returnDict.Add(arg.Name, $"?{argIndex++}");
            return returnDict;
        }

        private static void RenameActionArguments(GroundedAction action, Dictionary<string, string> replacements)
        {
            foreach (var arg in action.Arguments)
                if (replacements.ContainsKey(arg.Name))
                    arg.Name = replacements[arg.Name];
            foreach (var name in _RemoveNamesFromActions)
                action.ActionName = action.ActionName.Replace(name, "");
        }

        private static int IndexOfMetaAction(ActionPlan leaderPlan)
        {
            for (int i = 0; i < leaderPlan.Plan.Count; i++)
                if (leaderPlan.Plan[i].ActionName.Contains(_metaActionName))
                    return i;
            throw new Exception("No meta action found in plan!");
        }

        private static HashSet<RepairSequence> GenerateMacros(Dictionary<GroundedAction, HashSet<ActionPlan>> from, DomainDecl domain)
        {
            var returnList = new HashSet<RepairSequence>();

            foreach (var key in from.Keys)
            {
                foreach (var actionPlan in from[key])
                {
                    var macro = GenerateMacroInstance(key.ActionName, actionPlan, domain);
                    if (macro.Effects is AndExp and && and.Children.Count == 0)
                        continue;

                    int id = 0;
                    var changeParams = macro.Parameters.Values.Where(x => !x.Name.StartsWith("?"));
                    var replacementDict = new Dictionary<string, string>();
                    foreach (var arg in changeParams)
                        replacementDict.Add(arg.Name, $"?O{id}");
                    foreach (var arg in replacementDict.Keys)
                    {
                        var allRefs = macro.FindNames(arg);
                        foreach (var aRef in allRefs)
                            aRef.Name = $"?O{id}";
                        id++;
                    }

                    foreach (var step in actionPlan.Plan)
                        RenameActionArguments(step, replacementDict);

                    returnList.Add(new RepairSequence(key, macro, actionPlan));
                }
            }

            return returnList;
        }

        private static ActionDecl GenerateMacroInstance(string newName, ActionPlan plan, DomainDecl domain)
        {
            SimpleActionCombiner combiner = new SimpleActionCombiner();
            var planActionInstances = new List<ActionDecl>();
            foreach (var actionPlan in plan.Plan)
                planActionInstances.Add(GenerateActionInstance(actionPlan, domain));
            var combined = combiner.Combine(planActionInstances);
            combined.Name = newName.Replace(_metaActionName, _macroActionName);
            return combined;
        }

        private static ActionDecl GenerateActionInstance(GroundedAction action, DomainDecl domain)
        {
            action.ActionName = RemoveNumberSufix(action);
            ActionDecl target = domain.Actions.First(x => x.Name == action.ActionName).Copy();
            var allNames = target.FindTypes<NameExp>();
            for (int i = 0; i < action.Arguments.Count; i++)
            {
                var allRefs = allNames.Where(x => x.Name == target.Parameters.Values[i].Name).ToList();
                foreach (var referene in allRefs)
                    referene.Name = action.Arguments[i].Name;
            }
            return target;
        }

        // This is a very lazy solution...
        // I will deal with it later
        private static string RemoveNumberSufix(GroundedAction action)
        {
            for (int i = 0; i < 1000; i++)
                if (action.ActionName.EndsWith($"_{i}"))
                    return action.ActionName.Replace($"_{i}", "");
            return action.ActionName;
        }
    }
}
