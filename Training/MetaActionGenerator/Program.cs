using CommandLine;
using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using Tools;

namespace MetaActionGenerator
{
    internal class Program : BaseCLI
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(GenerateMetaActions)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        public static void GenerateMetaActions(Options opts)
        {
            opts.MacroActionPath = PathHelper.RootPath(opts.MacroActionPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            PathHelper.RecratePath(opts.OutputPath);

            ConsoleHelper.WriteLineColor("Verifying paths...");
            if (!Directory.Exists(opts.MacroActionPath))
                throw new FileNotFoundException($"Macro action path not found: {opts.MacroActionPath}");
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Generating meta actions...");
            IErrorListener listener = new ErrorListener();
            List<ActionDecl> macros = ParseMacros(opts.MacroActionPath, listener);
            var metaActionGenerator = new MetaActionGenerator();
            var metaActions = metaActionGenerator.GenerateMetaActions(macros);
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            ConsoleHelper.WriteLineColor("Renaming meta actions...");
            int counter = 1;
            foreach (var metaAction in metaActions)
                metaAction.Name = $"$meta_{counter++}";
            ConsoleHelper.WriteLineColor("Done!", ConsoleColor.Green);

            OutputActions(metaActions, opts.OutputPath, listener);
            ConsoleHelper.WriteLineColor($"A total of {metaActions.Count} meta action was found.", ConsoleColor.Green);
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
    }
}