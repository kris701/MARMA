using PDDLSharp.CodeGenerators;
using PDDLSharp.CodeGenerators.PDDL;
using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.FastDownward.Plans;
using PDDLSharp.Parsers.PDDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace MetaActions.Train.VerificationStrategies
{
    public abstract class BaseUsefulVerificationStrategy : BaseVarificationStrategy
    {
        internal string _tempUsefulPath = "useful-check";
        public BaseUsefulVerificationStrategy(string name, int runID, string tempPath, CancellationTokenSource token) : base(name, runID, tempPath, token)
        {
            _tempUsefulPath = Path.Combine(tempPath, _tempUsefulPath);
            PathHelper.RecratePath(_tempUsefulPath);
        }

        internal bool IsMetaActionUseful(FileInfo metaAction, List<FileInfo> problems, string tempFolder)
        {
            var listener = new ErrorListener();
            var planParser = new FDPlanParser(listener);

            int counter = 1;
            foreach (var problem in problems)
            {
                if (CancellationToken.IsCancellationRequested) return false;
                Print($"\t\tUseful check on problem '{problem.Name}' [{counter++}/{problems.Count}]", ConsoleColor.DarkMagenta);

                using (ArgsCaller fdCaller = new ArgsCaller("python3"))
                {
                    _activeProcess = fdCaller.Process;
                    fdCaller.StdOut += (s, o) => { };
                    fdCaller.StdErr += (s, o) => { };
                    fdCaller.Arguments.Add(PathHelper.RootPath("Dependencies/fast-downward/fast-downward.py"), "");
                    fdCaller.Arguments.Add("--alias", "lama-first");
                    fdCaller.Arguments.Add("--overall-time-limit", "5m");
                    fdCaller.Arguments.Add("--plan-file", "plan.plan");
                    fdCaller.Arguments.Add("metaDomain.pddl", "");
                    fdCaller.Arguments.Add(problem.FullName, "");
                    fdCaller.Process.StartInfo.WorkingDirectory = tempFolder;
                    if (fdCaller.Run() == 0)
                    {
                        var plan = planParser.Parse(new FileInfo(Path.Combine(tempFolder, "plan.plan")));
                        if (plan.Plan.Any(y => y.ActionName == metaAction.Name.Replace(metaAction.Extension, "")))
                            return true;
                    }
                }
            }

            return false;
        }

        internal void GenerateMetaDomain(FileInfo domainFile, FileInfo metaActionFile, string tempFolder)
        {
            IErrorListener listener = new ErrorListener();
            IParser<INode> parser = new PDDLParser(listener);
            ICodeGenerator<INode> generator = new PDDLCodeGenerator(listener);
            generator.Readable = true;

            var domain = parser.ParseAs<DomainDecl>(domainFile);
            var metaAction = parser.ParseAs<ActionDecl>(metaActionFile);
            domain.Actions.Add(metaAction);
            generator.Generate(domain, Path.Combine(tempFolder, "metaDomain.pddl"));
        }
    }
}
