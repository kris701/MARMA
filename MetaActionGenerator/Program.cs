﻿using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;
using PDDLSharp.Models.PDDL.Problem;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using PDDLSharp.Toolkit.MutexDetector;
using Tools;

namespace MetaActionGenerator
{
    internal class Program : BaseCLI
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<MetaActionGeneratorOptions>(args)
              .WithParsed(GenerateMetaActions)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        public static void GenerateMetaActions(MetaActionGeneratorOptions opts)
        {
            opts.DomainFilePath = PathHelper.RootPath(opts.DomainFilePath);
            opts.MacroActionPath = PathHelper.RootPath(opts.MacroActionPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Verifying paths...");
            if (!File.Exists(opts.DomainFilePath))
                throw new FileNotFoundException($"Domain file not found: {opts.DomainFilePath}");
            if (!Directory.Exists(opts.MacroActionPath))
                throw new FileNotFoundException($"Macro action path not found: {opts.MacroActionPath}");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            IErrorListener listener = new ErrorListener();

            var domain = ParseDomain(opts.DomainFilePath, listener);

            List<ActionDecl> macros = ParseMacros(opts.MacroActionPath, listener);

            List<ActionDecl> metaActions = new List<ActionDecl>();

            metaActions.AddRange(GenerateCandidates(macros, "Generating 'Remove Precondition Parameters' meta actions...", new RemovePreconditionParameters()));
            metaActions.AddRange(GenerateCandidates(macros, "Generating 'Remove Effect Parameters' meta actions...", new RemoveEffectParameters()));
            metaActions.AddRange(GenerateCandidates(macros, "Generating 'Remove Additional Effects' meta actions...", new RemoveAdditionalEffects()));

            metaActions = RemoveActionsBy(metaActions, "Sanetizing meta actions...",
                (acts) =>
                {
                    acts.RemoveAll(x => (x.Effects is IWalkable effWalk && effWalk.Count() == 0));
                    acts.RemoveAll(x => (x.Effects is IListable effList && effList.Count() == 0));
                    return acts;
                });
            metaActions = RemoveActionsBy(metaActions, "Removing duplicate meta actions...",
                (acts) =>
                {
                    return acts.DistinctBy(x => x.GetHashCode()).ToList();
                });
            metaActions = RemoveActionsBy(metaActions, "Removing meta actions with bad mutex groups...",
                (acts) =>
                {
                    var newActs = new List<ActionDecl>();
                    IMutexDetectors mutexDetector = new EffectBalanceMutexes();
                    var mutexPredicates = mutexDetector.FindMutexes(new PDDLDecl(domain, new ProblemDecl()));
                    foreach (var action in acts)
                    {
                        bool isGood = true;
                        var predicates = action.Effects.FindTypes<PredicateExp>();
                        foreach (var mutex in mutexPredicates)
                        {
                            var possitives = predicates.Count(x => x.Name == mutex.Name && x.Parent is not NotExp);
                            var negatives = predicates.Count(x => x.Name == mutex.Name && x.Parent is NotExp);
                            if (possitives != negatives)
                            {
                                isGood = false;
                                break;
                            }
                        }
                        if (isGood)
                            newActs.Add(action);
                    }
                    return newActs;
                });


            ConsoleHelper.WriteLineColor("Renaming meta actions...");
            int counter = 1;
            foreach (var metaAction in metaActions)
                metaAction.Name = $"$meta_{counter++}";
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            OutputActions(metaActions, opts.OutputPath, listener);
            ConsoleHelper.WriteLineColor($"A total of {metaActions.Count} meta action was found.", ConsoleColor.Green);
        }

        private static DomainDecl ParseDomain(string path, IErrorListener listener)
        {
            ConsoleHelper.WriteLineColor("Parsing domain...");
            IParser<INode> parser = new PDDLParser(listener);
            var domain = parser.ParseAs<DomainDecl>(new FileInfo(path));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return domain;
        }

        private static List<ActionDecl> ParseMacros(string path, IErrorListener listener)
        {
            IParser<INode> parser = new PDDLParser(listener);
            ConsoleHelper.WriteLineColor("Parsing macros...");
            List<ActionDecl> macros = new List<ActionDecl>();
            foreach (var file in new DirectoryInfo(path).GetFiles())
                if (file.Extension == ".pddl")
                    macros.Add(parser.ParseAs<ActionDecl>(file));
            ConsoleHelper.WriteLineColor($"A total of {macros.Count} macros was found.", ConsoleColor.Green);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return macros;
        }

        private static void OutputActions(List<ActionDecl> actions, string outPath, IErrorListener listener)
        {
            ConsoleHelper.WriteLineColor("Outputting files...");
            ICodeGenerator<INode> generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            foreach (var metaAction in actions)
                generator.Generate(metaAction, Path.Combine(outPath, $"{metaAction.Name}.pddl"));
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }

        private static List<ActionDecl> GenerateCandidates(List<ActionDecl> macros, string info, ICandidateGenerator generator)
        {
            ConsoleHelper.WriteLineColor(info);
            var items = generator.Generate(macros);
            ConsoleHelper.WriteLineColor($"Generated {items.Count} candidates.");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return items;
        }

        private static List<ActionDecl> RemoveActionsBy(List<ActionDecl> actions, string info, Func<List<ActionDecl>, List<ActionDecl>> by)
        {
            ConsoleHelper.WriteLineColor(info);
            int preCount = actions.Count;
            var newActions = by(actions);
            ConsoleHelper.WriteLineColor($"Removed {preCount - newActions.Count} actions out of {preCount} [{100 - Math.Round(((double)newActions.Count / (double)preCount) * 100, 0)}%]");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
            return newActions;
        }
    }
}