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
            opts.DomainPath = PathHelper.RootPath(opts.DomainPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);
            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Parsing domain...");
            var domain = ParseDomain(opts.DomainPath);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Extracting plan sequences...");
            var planSequences = ExtractUniquePlanSequences(opts.FollowerPlans.ToList());
            ConsoleHelper.WriteLineColor($"A total of {planSequences.Keys.Count} unique meta action instances found.");
            ConsoleHelper.WriteLineColor($"A total of {planSequences.Sum(x => x.Value.Count)} replacement sequences found.");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating macro sequences...");
            var macros = GenerateMacros(planSequences, domain);
            ConsoleHelper.WriteLineColor($"A total of {macros.Sum(x => x.Value.Count)} unique macros found.");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting reconstruction data...");
            OutputReconstructionData(macros, opts.OutputPath);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }

        private static DomainDecl ParseDomain(string domainFile)
        {
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);
            return parser.ParseAs<DomainDecl>(new FileInfo(domainFile));
        }

        private static Dictionary<GroundedAction, HashSet<ActionPlan>> ExtractUniquePlanSequences(List<string> followerPlanFiles)
        {
            var followerPlans = PathHelper.ResolveWildcards(followerPlanFiles);

            var planSequences = new Dictionary<GroundedAction, HashSet<ActionPlan>>();
            IErrorListener listener = new ErrorListener();
            IParser<ActionPlan> parser = new FastDownwardPlanParser(listener);

            foreach(var planFile in followerPlans)
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
                returnDict.Add(arg.Name, $"?{argIndex++}");
            return returnDict;
        }

        private static void RenameActionArguments(GroundedAction action, Dictionary<string, string> replacements)
        {
            foreach (var arg in action.Arguments)
                arg.Name = replacements[arg.Name];
            if (action.ActionName.StartsWith("attack_") || action.ActionName.StartsWith("fix_"))
                action.ActionName = action.ActionName.Replace("attack_", "").Replace("fix_", "");
        }

        private static int IndexOfMetaAction(ActionPlan leaderPlan)
        {
            for (int i = 0; i < leaderPlan.Plan.Count; i++)
                if (leaderPlan.Plan[i].ActionName.StartsWith("fix_$meta"))
                    return i;
            throw new Exception("No meta action found in leader plan!");
        }

        private static Dictionary<GroundedAction, HashSet<RepairSequence>> GenerateMacros(Dictionary<GroundedAction, HashSet<ActionPlan>> from, DomainDecl domain)
        {
            var returnDict = new Dictionary<GroundedAction, HashSet<RepairSequence>>();

            foreach(var key in from.Keys)
            {
                if (!returnDict.ContainsKey(key))
                    returnDict.Add(key, new HashSet<RepairSequence>());
                foreach (var actionPlan in from[key])
                {
                    var macro = GenerateMacroInstance(key.ActionName, actionPlan, domain);
                    if (macro.Parameters.Values.Count != key.Arguments.Count)
                        throw new ArgumentException("Macro is invalid! It does not have the same amount of parameters as the meta action it replaces.");
                    returnDict[key].Add(new RepairSequence(macro, actionPlan));
                }
            }

            return returnDict;
        }

        private static ActionDecl GenerateMacroInstance(string newName, ActionPlan plan, DomainDecl domain)
        {
            SimpleActionCombiner combiner = new SimpleActionCombiner();
            var planActionInstances = new List<ActionDecl>();
            foreach(var actionPlan in plan.Plan) 
                planActionInstances.Add(GenerateActionInstance(actionPlan, domain));
            var combined = combiner.Combine(planActionInstances);
            combined.Name = newName.Replace("$meta", "$macro");
            return combined;
        }

        private static ActionDecl GenerateActionInstance(GroundedAction action, DomainDecl domain)
        {
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

        private static void OutputReconstructionData(Dictionary<GroundedAction, HashSet<RepairSequence>> repairSequences, string outPath)
        {
            IErrorListener listener = new ErrorListener();
            ICodeGenerator<INode> codeGenerator = new PDDLCodeGenerator(listener);
            ICodeGenerator<ActionPlan> planGenerator = new FastDownwardPlanGenerator(listener);
            foreach (var key in repairSequences.Keys)
            {
                PathHelper.RecratePath(Path.Combine(outPath, key.ActionName));
                int id = 1;
                foreach (var replacement in repairSequences[key])
                {
                    codeGenerator.Generate(replacement.Macro, Path.Combine(outPath, key.ActionName, $"macro{id}.pddl"));
                    planGenerator.Generate(replacement.Replacement, Path.Combine(outPath, key.ActionName, $"macro{id}_replacement.plan"));
                    id++;
                }
            }
        }
    }
}