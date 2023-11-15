using CommandLine;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.CodeGenerators.Plans;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers.PDDL;
using Tools;

namespace MacroExtractor
{
    internal class Program : BaseCLI
    {
        public static string _metaActionName = "$meta";
        public static string _macroActionName = "$macro";
        public static string[] _RemoveNamesFromActions = { "attack_", "fix_" };

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

            ConsoleHelper.WriteLineColor("Parsing domain...");
            var domain = ParseDomain(opts.DomainPath);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating macro sequences...");
            var extractor = new MacroExtractor();
            var macros = extractor.ExtractMacros(domain, opts.FollowerPlans.ToList());
            ConsoleHelper.WriteLineColor($"A total of {macros.DistinctBy(x => x.MetaAction).Count()} unique meta actions found.");
            ConsoleHelper.WriteLineColor($"A total of {macros.Count} macros and replacements found.");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Outputting reconstruction data...");
            OutputReconstructionData(macros, opts.OutputPath);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);
        }

        private static DomainDecl ParseDomain(string domainFile)
        {
            var listener = new ErrorListener();
            var parser = new PDDLParser(listener);
            return parser.ParseAs<DomainDecl>(new FileInfo(domainFile));
        }

        private static void OutputReconstructionData(List<RepairSequence> repairSequences, string outPath)
        {
            var listener = new ErrorListener();
            var codeGenerator = new PDDLCodeGenerator(listener);
            var planGenerator = new FastDownwardPlanGenerator(listener);
            foreach (var item in repairSequences)
                PathHelper.RecratePath(Path.Combine(outPath, item.MetaAction.ActionName));
            int id = 1;
            foreach (var replacement in repairSequences)
            {
                codeGenerator.Generate(replacement.Macro, Path.Combine(outPath, replacement.MetaAction.ActionName, $"macro{id}.pddl"));
                planGenerator.Generate(replacement.Replacement, Path.Combine(outPath, replacement.MetaAction.ActionName, $"macro{id}_replacement.plan"));
                id++;
            }
        }
    }
}