﻿using CommandLine;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Tools;

namespace MetaActions.Learn
{
    internal class Program : BaseCLI
    {
        private static string _tempProblemPath = "problems";
        private static string _tempMacroPath = "macros";
        private static string _tempMacroTempPath = "macrosTemp";
        private static string _tempMetaActionPath = "metaActions";
        private static string _tempCompiledPath = "compiled";
        private static string _tempVerificationPath = "verification";
        private static string _outValidMetaActionPath = "valid";

        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(Run)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        private static void Run(Options opts)
        {
            opts.DomainPath = PathHelper.RootPath(opts.DomainPath);
            opts.TempPath = PathHelper.RootPath(opts.TempPath);
            opts.OutputPath = PathHelper.RootPath(opts.OutputPath);

            RecratePath(opts.TempPath);
            RecratePath(opts.OutputPath);

            _tempProblemPath = Path.Combine(opts.TempPath, _tempProblemPath);
            _tempMacroPath = Path.Combine(opts.TempPath, _tempMacroPath);
            _tempMacroTempPath = Path.Combine(opts.TempPath, _tempMacroTempPath);
            _tempMetaActionPath = Path.Combine(opts.TempPath, _tempMetaActionPath);
            _tempCompiledPath = Path.Combine(opts.TempPath, _tempCompiledPath);
            _outValidMetaActionPath = Path.Combine(opts.OutputPath, _outValidMetaActionPath);

            RecratePath(_tempProblemPath);
            RecratePath(_tempMacroPath);
            RecratePath(_tempMacroTempPath);
            RecratePath(_tempMetaActionPath);
            RecratePath(_tempCompiledPath);
            RecratePath(_outValidMetaActionPath);

            var problems = CopyProblemsToTemp(opts.Problems);

            ConsoleHelper.WriteLineColor($"Generating macros", ConsoleColor.Gray);
            List<FileInfo> allMacros = GenerateMacros(opts.DomainPath);
            ConsoleHelper.WriteLineColor($"A total of {allMacros.Count} macros was found.", ConsoleColor.Gray);

            ConsoleHelper.WriteLineColor($"Generating meta actions", ConsoleColor.Gray);
            List<FileInfo> allMetaActions = GenerateMetaActions(opts.DomainPath);
            ConsoleHelper.WriteLineColor($"A total of {allMetaActions.Count} meta actions was found.", ConsoleColor.Gray);

            ConsoleHelper.WriteLineColor($"Testing meta actions", ConsoleColor.Gray);
            int totalValidMetaActions = 0;
            int metaActionCounter = 1;
            foreach (var metaAction in allMetaActions)
            {
                int problemCounter = 1;
                bool allValid = true;
                foreach (var problem in problems)
                {
                    ConsoleHelper.WriteLineColor($"Testing meta action {metaActionCounter} of {allMetaActions.Count} on problem {problemCounter++} out of {problems.Count}.", ConsoleColor.Gray);
                    // Compile Meta Actions
                    ConsoleHelper.WriteLineColor($"Compiling meta action.", ConsoleColor.Gray);
                    CompileMetaAction(opts.DomainPath, problem.FullName, metaAction.FullName);

                    // Verify Meta Actions
                    ConsoleHelper.WriteLineColor($"Verifying meta action.", ConsoleColor.Gray);
                    var isMetaActionValid = VerifyMetaAction();

                    // Stop if invalid
                    if (!isMetaActionValid) 
                    { 
                        ConsoleHelper.WriteLineColor($"Meta action was invalid.", ConsoleColor.Red);
                        allValid = false;
                        break;
                    }
                }
                if (allValid)
                {
                    ConsoleHelper.WriteLineColor($"Meta action was valid.", ConsoleColor.Green);
                    totalValidMetaActions++;
                    File.Copy(metaAction.FullName, Path.Combine(_outValidMetaActionPath, $"meta{metaActionCounter}.pddl"));
                }
                metaActionCounter++;
            }
            ConsoleHelper.WriteLineColor($"A total of {totalValidMetaActions} valid meta actions was found.", ConsoleColor.Green);
        }

        private static void RecratePath(string path)
        {
            if (Directory.Exists(path))
                new DirectoryInfo(path).Delete(true);
            Directory.CreateDirectory(path);
        }

        private static List<FileInfo> CopyProblemsToTemp(IEnumerable<string> allProblems)
        {
            var problems = new List<FileInfo>();
            foreach (var problem in allProblems)
            {
                var file = new FileInfo(PathHelper.RootPath(problem));
                File.Copy(file.FullName, Path.Combine(_tempProblemPath, file.Name));
                problems.Add(new FileInfo(Path.Combine(_tempProblemPath, file.Name)));
            }
            return problems;
        }

        private static List<FileInfo> GenerateMacros(string domain)
        {
            var macroGenerator = GetRustRunner("macros");
            macroGenerator.Arguments.Add("-d", domain);
            macroGenerator.Arguments.Add("-p", _tempProblemPath);
            macroGenerator.Arguments.Add("-o", _tempMacroPath);
            macroGenerator.Arguments.Add("-t", _tempMacroTempPath);
            macroGenerator.Arguments.Add("-c", PathHelper.RootPath("Dependencies/CSMs"));
            macroGenerator.Arguments.Add("-f", PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"));
            if (macroGenerator.Run() != 0)
                throw new Exception("Macro generation failed!");
            return new DirectoryInfo(_tempMacroPath).GetFiles().ToList();
        }

        private static List<FileInfo> GenerateMetaActions(string domain)
        {
            ArgsCaller metaCaller = GetDotnetRunner("MetaActionGenerator");
            metaCaller.Arguments.Add("--domain", domain);
            metaCaller.Arguments.Add("--macros", _tempMacroPath);
            metaCaller.Arguments.Add("--output", _tempMetaActionPath);
            if (metaCaller.Run() != 0)
                throw new Exception("Meta action generation failed!");
            return new DirectoryInfo(_tempMetaActionPath).GetFiles().ToList();
        }

        private static void CompileMetaAction(string domain, string problem, string metaAction)
        {
            ArgsCaller stackelCompiler = GetDotnetRunner("StacklebergCompiler");
            stackelCompiler.Arguments.Add("--domain", domain);
            stackelCompiler.Arguments.Add("--problem", problem);
            stackelCompiler.Arguments.Add("--meta-action", metaAction);
            stackelCompiler.Arguments.Add("--output", _tempCompiledPath);
            if (stackelCompiler.Run() != 0)
                throw new Exception("Stackelberg Compiler failed!");
        }

        private static bool VerifyMetaAction()
        {
            ArgsCaller stackelVerifier = GetDotnetRunner("StackelbergVerifier");
            stackelVerifier.Arguments.Add("--domain", Path.Combine(_tempCompiledPath, "simplified_domain.pddl"));
            stackelVerifier.Arguments.Add("--problem", Path.Combine(_tempCompiledPath, "simplified_problem.pddl"));
            stackelVerifier.Arguments.Add("--output", _tempVerificationPath);
            stackelVerifier.Arguments.Add("--stackelberg", PathHelper.RootPath("Dependencies/stackelberg-planner/src/fast-downward.py"));
            var code = stackelVerifier.Run();
            if (code != 0 && code != 1)
                throw new Exception("Stackelberg verifier failed!");
            return code == 0;
        }

        private static ArgsCaller GetDotnetRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller("dotnet");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("run", "");
            runner.Arguments.Add("--configuration", "Release");
            runner.Arguments.Add("--project", $"{project} --");
            return runner;
        }

        private static ArgsCaller GetRustRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller("cargo");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("run", "");
            runner.Arguments.Add("--release", "");
            runner.Arguments.Add("--manifest-path", $"{project}/Cargo.toml --");
            return runner;
        }

        private static void PrintStdOut(object sender, DataReceivedEventArgs e)
        {
#if DEBUG
            if (e.Data != null)
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Gray);
#endif
        }

        private static void PrintStdErr(object sender, DataReceivedEventArgs e)
        {
#if DEBUG
            if (e.Data != null)
                ConsoleHelper.WriteLineColor(e.Data, ConsoleColor.Red);
#endif
        }
    }
}