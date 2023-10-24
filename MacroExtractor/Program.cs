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

            ConsoleHelper.WriteLineColor("Matching plans...");
            var leaderFollowerPairs = GetLeaderFollowerFilePairs(opts.LeaderPlans.ToList(), opts.FollowerPlans.ToList());
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Extracting plan sequences...");
            var planSequences = ExtractUniquePlanSequences(leaderFollowerPairs);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating macro sequences...");
            var macros = GenerateMacros(planSequences, domain);
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

        private static Dictionary<GroundedAction, HashSet<ActionPlan>> ExtractUniquePlanSequences(Dictionary<FileInfo, List<FileInfo>> leaderFollowerPairs)
        {
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
            return macros;
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

        private static Dictionary<GroundedAction, List<ActionDecl>> GenerateMacros(Dictionary<GroundedAction, HashSet<ActionPlan>> from, DomainDecl domain)
        {
            var returnDict = new Dictionary<GroundedAction, List<ActionDecl>>();

            foreach(var key in from.Keys)
            {
                returnDict.Add(key, new List<ActionDecl>());
                foreach (var actionPlan in from[key])
                {
                    var macro = GenerateMacroInstance(key.ActionName, actionPlan, domain);
                    if (macro.Parameters.Values.Count != key.Arguments.Count)
                        throw new ArgumentException("Macro is invalid! It does not have the same amount of parameters as the meta action it replaces.");
                    returnDict[key].Add(macro);
                }
                returnDict[key] = returnDict[key].Distinct().ToList();
            }

            return returnDict;
        }

        private static ActionDecl GenerateMacroInstance(string newName, ActionPlan plan, DomainDecl domain)
        {
            SimpleActionCombiner combiner = new SimpleActionCombiner();
            List<ActionDecl> planActionInstances = new List<ActionDecl>();
            foreach(var actionPlan in plan.Plan) 
                planActionInstances.Add(GenerateActionInstance(actionPlan, domain));
            var combined = combiner.Combine(planActionInstances);
            combined.Name = newName;
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

        private static void OutputReconstructionData(Dictionary<GroundedAction, List<ActionDecl>> macros, string outPath)
        {
            IErrorListener listener = new ErrorListener();
            ICodeGenerator<INode> codeGenerator = new PDDLCodeGenerator(listener);
            foreach (var key in macros.Keys)
            {
                PathHelper.RecratePath(Path.Combine(outPath, key.ActionName));
                int id = 1;
                foreach (var replacement in macros[key])
                    codeGenerator.Generate(replacement, Path.Combine(outPath, key.ActionName, $"macro{id++}.pddl"));
            }
        }
    }
}