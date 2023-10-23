﻿using CommandLine;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.Plans;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Toolkit.MacroGenerators;
using System;
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
            ConsoleHelper.WriteLineColor("");

            List<FileInfo> leaderPlans = new List<FileInfo>();
            foreach (var plan in opts.LeaderPlans)
                leaderPlans.Add(new FileInfo(PathHelper.RootPath(plan)));
            List<FileInfo> followerPlans = new List<FileInfo>();
            foreach (var plan in opts.FollowerPlans)
                followerPlans.Add(new FileInfo(PathHelper.RootPath(plan)));

            List<PlanPair> planPairs = new List<PlanPair>();
            foreach(var leaderPlan in leaderPlans)
            {
                var matches = followerPlans.Where(x => x.Name == leaderPlan.Name);
                foreach (var match in matches)
                    planPairs.Add(new PlanPair(leaderPlan, match));
            }

            var domain = ParseDomain(opts.Domain);

            List<ReconstructionPair> macros = new List<ReconstructionPair>();
            SimpleActionCombiner combiner = new SimpleActionCombiner();
            foreach (var pair in planPairs)
            {
                int index = 0;
                for(int i = 0; i < pair.FollowerPlan.Plan.Count; i++)
                {
                    if (!pair.FollowerPlan.Plan[i].Equals(pair.LeaderPlan.Plan[i]))
                    {
                        index = i;
                        break;
                    }
                }
                var groundedMacroSequence = pair.FollowerPlan.Plan.GetRange(index, pair.FollowerPlan.Plan.Count - index);
                var macroSequence = GenerateSequence(groundedMacroSequence, domain);
                var macro = combiner.Combine(macroSequence);

                macros.Add(new ReconstructionPair(macro, new ActionSequence(groundedMacroSequence)));
            }
        }

        private static DomainDecl ParseDomain(string file)
        {
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);
            return parser.ParseAs<DomainDecl>(new FileInfo(file));
        }

        private static List<ActionDecl> GenerateSequence(List<GroundedAction> groundedActions, DomainDecl decl)
        {
            List<ActionDecl> actions = new List<ActionDecl>();
            foreach (var groundedAction in groundedActions)
                actions.Add(GenerateActionInstance(groundedAction, decl));
            return actions;
        }

        private static ActionDecl GenerateActionInstance(GroundedAction action, DomainDecl decl)
        {
            ActionDecl target = decl.Actions.First(x => x.Name == action.ActionName).Copy();
            var allNames = target.FindTypes<NameExp>();
            for (int i = 0; i < action.Arguments.Count; i++)
            {
                var allRefs = allNames.Where(x => x.Name == target.Parameters.Values[i].Name).ToList();
                foreach (var referene in allRefs)
                    referene.Name = $"?{action.Arguments[i].Name}";
            }
            return target;
        }
    }
}