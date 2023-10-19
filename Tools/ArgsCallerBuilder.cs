using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class ArgsCallerBuilder
    {
        public static ArgsCaller GetDotnetRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller("dotnet");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("run", "");
            runner.Arguments.Add("--configuration", "Release");
            runner.Arguments.Add("--project", $"{project} --");
            return runner;
        }

        public static ArgsCaller GetRustRunner(string project)
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
                ConsoleHelper.WriteLineColor(e.Data);
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
