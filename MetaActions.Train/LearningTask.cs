using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.PDDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Learn
{
    public class LearningTask
    {
        private string _tempProblemPath = "problems";
        private string _tempMacroPath = "macros";
        private string _tempMacroTempPath = "macrosTemp";
        private string _tempCSMPath = "CSMsTemp";
        private string _tempMetaActionPath = "metaActions";
        private string _tempCompiledPath = "compiled";
        private string _tempVerificationPath = "verification";
        private string _tempReplacementsPath = "replacements";

        private string _outProblems = "problems";
        private string _outCache = "cache";

        private string _domainName = "";

        private string _cachePath = "cache";
        private string _macroCachePath = "macros";

        private int _runHash = -1;

        private static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public string LearnDomain(string tempPath, string outPath, FileInfo domain, List<FileInfo> trainProblems, List<FileInfo> testProblems)
        {
            if (domain.Directory == null)
                throw new FileNotFoundException("Domain does not have a parent directory!");
            _domainName = domain.Directory.Name;

            _runHash = GetDeterministicHashCode(domain.FullName).GetHashCode();
            foreach(var trainProblem in trainProblems)
                _runHash ^= GetDeterministicHashCode(trainProblem.FullName).GetHashCode();

            Print($"Training started", ConsoleColor.Blue);

            PathHelper.RecratePath(tempPath);
            PathHelper.RecratePath(outPath);

            _cachePath = PathHelper.RootPath(_cachePath);
            _macroCachePath = Path.Combine(_cachePath, _macroCachePath);

            _tempProblemPath = Path.Combine(tempPath, _tempProblemPath);
            _tempMacroPath = Path.Combine(tempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(tempPath, _tempMacroTempPath);
            _tempCSMPath = Path.Combine(tempPath, _tempCSMPath);
            _tempMetaActionPath = Path.Combine(tempPath, _tempMetaActionPath);
            _tempCompiledPath = Path.Combine(tempPath, _tempCompiledPath);
            _tempVerificationPath = Path.Combine(tempPath, _tempVerificationPath);
            _tempReplacementsPath = Path.Combine(_tempVerificationPath, _tempReplacementsPath);

            _outProblems = Path.Combine(outPath, _outProblems);
            _outCache = Path.Combine(outPath, _outCache);

            PathHelper.RecratePath(_tempProblemPath);
            PathHelper.RecratePath(_tempMacroPath);
            PathHelper.RecratePath(_tempMacroTempPath);
            PathHelper.RecratePath(_tempCSMPath);
            PathHelper.RecratePath(_tempMetaActionPath);
            PathHelper.RecratePath(_tempCompiledPath);
            PathHelper.RecratePath(_tempVerificationPath);
            PathHelper.RecratePath(_tempReplacementsPath);

            PathHelper.RecratePath(_outProblems);
            PathHelper.RecratePath(_outCache);

            var problems = CopyProblemsToTemp(trainProblems);

            Print($"There is a total of {problems.Count} problems to train with.", ConsoleColor.Blue);

            Print($"Generating macros", ConsoleColor.Blue, false);
            // Make a temp copy of CSMs, since it cant handle multiple runs at the same time.
            CopyFilesRecursively(PathHelper.RootPath("Dependencies/CSMs/src"), Path.Combine(_tempCSMPath, "src"));
            CopyFilesRecursively(PathHelper.RootPath("Dependencies/CSMs/scripts"), Path.Combine(_tempCSMPath, "scripts"));
            List<FileInfo> allMacros = GenerateMacros(domain.FullName);
            Print($"A total of {allMacros.Count} macros was found.", ConsoleColor.Blue, false);
            if (allMacros.Count == 0)
                return _domainName;

            Print($"Generating meta actions", ConsoleColor.Blue, false);
            List<FileInfo> allMetaActions = GenerateMetaActions();
            Print($"A total of {allMetaActions.Count} meta actions was found.", ConsoleColor.Blue, false);
            if (allMetaActions.Count == 0)
                return _domainName;

            Print($"Validating meta actions", ConsoleColor.Blue, false);
            List<FileInfo> validMetaActions = new List<FileInfo>();
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                PathHelper.RecratePath(_tempReplacementsPath);
                Print($"\tTesting meta action {metaActionCounter} of {allMetaActions.Count} [{Math.Round(((double)metaActionCounter / (double)allMetaActions.Count) * 100, 0)}%]", ConsoleColor.Magenta);
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in problems)
                {
                    Print($"\t\tProblem {problemCounter} out of {problems.Count} [{Math.Round(((double)problemCounter / (double)problems.Count) * 100, 0)}%].", ConsoleColor.DarkMagenta);
                    // Compile Meta Actions
                    Print($"\t\tCompiling meta action.", ConsoleColor.DarkMagenta);
                    CompileMetaAction(domain.FullName, problem.FullName, metaAction.FullName);

                    // Verify Meta Actions
                    Print($"\t\tVerifying meta action.", ConsoleColor.DarkMagenta);
                    var isMetaActionValid = VerifyMetaAction();

                    // Stop if invalid
                    if (!isMetaActionValid)
                    {
                        Print($"\tMeta action was invalid in problem '{problem.Name}'.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                    problemCounter++;
                }
                if (allValid)
                {
                    Print($"\tMeta action was valid in all {problems.Count} problems.", ConsoleColor.Green);
                    validMetaActions.Add(metaAction);
                    Print($"Extracting macros from plans...", ConsoleColor.Blue);

                    ExtractMacrosFromPlans(domain, _tempReplacementsPath, _outCache);

                    Print($"Done!", ConsoleColor.Green);
                }
                metaActionCounter++;
            }
            Print($"A total of {validMetaActions.Count} valid meta actions out of {allMetaActions.Count} was found.", ConsoleColor.Green, false);
            Print($"Generating meta domain...", ConsoleColor.Blue);

            GenerateMetaDomain(domain, validMetaActions, outPath);

            Print($"Done!", ConsoleColor.Green);

            Print($"Copying testing problems...", ConsoleColor.Blue);

            CopyTestingProblems(testProblems, _outProblems);

            Print($"Done!", ConsoleColor.Green);

            return _domainName;
        }

        private void Print(string text, ConsoleColor color, bool debugOnly = true)
        {
# if DEBUG
            ConsoleHelper.WriteLineColor($"\t[{_domainName}] {text}", color);
# endif
            if (!debugOnly)
            {
#if !DEBUG
            ConsoleHelper.WriteLineColor($"\t[{_domainName}] {text}", color);
#endif
            }
        }

        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            PathHelper.RecratePath(targetPath);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                var target = newPath.Replace(sourcePath, targetPath);
                int id = 0;
                while (File.Exists(target))
                    target = target.Replace(".pddl", $"{id++}.pddl");
                File.Copy(newPath, target, true);
            }
        }

        private void CopyTestingProblems(List<FileInfo> testingProblems, string outFolder)
        {
            foreach (var problem in testingProblems)
            {
                var target = new FileInfo(Path.Combine(outFolder, problem.Name));
                while (File.Exists(target.FullName))
                {
                    if (problem.Directory == null)
                        throw new FileNotFoundException("File was not in a directory?");
                    target = new FileInfo(target.FullName.Replace(target.Name.Replace(target.Extension, ""), $"{target.Name.Replace(target.Extension, "")}_{problem.Directory.Name}"));
                }
                File.Copy(problem.FullName, target.FullName);
            }
        }

        private void GenerateMetaDomain(FileInfo domainFile, List<FileInfo> metaActionFiles, string outFolder)
        {
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);
            ICodeGenerator<INode> generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            var domain = parser.ParseAs<DomainDecl>(domainFile);
            generator.Generate(domain, Path.Combine(outFolder, "domain.pddl"));

            foreach (var file in metaActionFiles)
            {
                var metaAction = parser.ParseAs<ActionDecl>(file);
                domain.Actions.Add(metaAction);
            }
            generator.Generate(domain, Path.Combine(outFolder, "metaDomain.pddl"));
        }

        private void ExtractMacrosFromPlans(FileInfo domain, string macroPlans, string outFolder)
        {
            ArgsCaller macroExtractor = ArgsCallerBuilder.GetDotnetRunner("MacroExtractor");
            macroExtractor.Arguments.Add("--domain", domain.FullName);
            string macroPlansStr = "";
            var planFiles = new DirectoryInfo(macroPlans).GetFiles();
            if (planFiles.Count() == 0)
                throw new Exception("Error, there where no plans made from the stackelberg planner");
            foreach (var plan in planFiles)
                macroPlansStr += $" {plan.FullName}";
            macroExtractor.Arguments.Add("--follower-plans", macroPlansStr);
            macroExtractor.Arguments.Add("--output", outFolder);
            if (macroExtractor.Run() != 0)
                throw new Exception("Macro Extractor failed!");
        }

        private List<FileInfo> CopyProblemsToTemp(List<FileInfo> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                File.Copy(problem.FullName, Path.Combine(_tempProblemPath, problem.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, problem.Name)));
            }
            return problems;
        }

        private List<FileInfo> GenerateMacros(string domain)
        {
            var checkPath = Path.Combine(_macroCachePath, _runHash.ToString());
            if (Directory.Exists(checkPath) && _runHash != -1)
            {
                CopyFilesRecursively(checkPath, _tempMacroPath);
            }
            else
            {
                var macroGenerator = ArgsCallerBuilder.GetRustRunner("macros");
                macroGenerator.Arguments.Add("-d", domain);
                macroGenerator.Arguments.Add("-p", _tempProblemPath);
                macroGenerator.Arguments.Add("-o", _tempMacroPath);
                macroGenerator.Arguments.Add("-t", _tempMacroTempPath);
                macroGenerator.Arguments.Add("-c", _tempCSMPath);
                macroGenerator.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
                if (macroGenerator.Run() != 0)
                    throw new Exception("Macro generation failed!");

                PathHelper.RecratePath(checkPath);
                CopyFilesRecursively(_tempMacroPath, checkPath);
            }
            return new DirectoryInfo(_tempMacroPath).GetFiles().ToList();
        }

        private List<FileInfo> GenerateMetaActions()
        {
            ArgsCaller metaCaller = ArgsCallerBuilder.GetDotnetRunner("MetaActionGenerator");
            metaCaller.Arguments.Add("--macros", _tempMacroPath);
            metaCaller.Arguments.Add("--output", _tempMetaActionPath);
            if (metaCaller.Run() != 0)
                throw new Exception("Meta action generation failed!");
            return new DirectoryInfo(_tempMetaActionPath).GetFiles().ToList();
        }

        private void CompileMetaAction(string domain, string problem, string metaAction)
        {
            ArgsCaller stackelCompiler = ArgsCallerBuilder.GetDotnetRunner("StackelbergCompiler");
            stackelCompiler.Arguments.Add("--domain", domain);
            stackelCompiler.Arguments.Add("--problem", problem);
            stackelCompiler.Arguments.Add("--meta-action", metaAction);
            stackelCompiler.Arguments.Add("--output", _tempCompiledPath);
            if (stackelCompiler.Run() != 0)
                throw new Exception("Stackelberg Compiler failed!");
        }

        private bool VerifyMetaAction()
        {
            ArgsCaller stackelVerifier = ArgsCallerBuilder.GetDotnetRunner("StackelbergVerifier");
            stackelVerifier.Arguments.Add("--domain", Path.Combine(_tempCompiledPath, "simplified_domain.pddl"));
            stackelVerifier.Arguments.Add("--problem", Path.Combine(_tempCompiledPath, "simplified_problem.pddl"));
            stackelVerifier.Arguments.Add("--output", _tempVerificationPath);
            stackelVerifier.Arguments.Add("--iseasy", "");
            stackelVerifier.Arguments.Add("--stackelberg", PathHelper.RootPath("Dependencies/stackelberg-planner/src/fast-downward.py"));
            var code = stackelVerifier.Run();
            if (code != 0 && code != 1)
                throw new Exception("Stackelberg verifier failed!");
            return code == 0;
        }
    }
}
