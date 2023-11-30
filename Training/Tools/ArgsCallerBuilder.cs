using System.Diagnostics;

namespace Tools
{
    public static class ArgsCallerBuilder
    {
        public static ArgsCaller GetGenericRunner(string executable)
        {
            ArgsCaller runner = new ArgsCaller(executable);
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            return runner;
        }

        public static ArgsCaller GetDotnetRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller($"{project}/bin/Release/net7.0/{project}");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            return runner;
        }

        public static ArgsCaller GetDotnetBuilder(string project)
        {
            ArgsCaller runner = new ArgsCaller("dotnet");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("build", "");
            runner.Arguments.Add("--configuration", "Release");
            runner.Arguments.Add(project, "");
            return runner;
        }

        public static ArgsCaller GetRustRunner(string project)
        {
            ArgsCaller runner = new ArgsCaller($"{project}/target/release/{project}");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            return runner;
        }

        public static ArgsCaller GetRustBuilder(string project)
        {
            ArgsCaller runner = new ArgsCaller("cargo");
            runner.StdOut += PrintStdOut;
            runner.StdErr += PrintStdErr;
            runner.Arguments.Add("build", "");
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
