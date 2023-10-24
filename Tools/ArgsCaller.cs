using System.Diagnostics;
using System.Text;

namespace Tools
{
    public class ArgsCaller
    {
        public event DataReceivedEventHandler? StdErr;
        public event DataReceivedEventHandler? StdOut;

        public string Program { get; }
        public Dictionary<string, string> Arguments { get; }
        public Dictionary<string, string> Environment { get; }

        public ArgsCaller(string program, Dictionary<string, string> arguments)
        {
            Program = program;
            Arguments = arguments;
            Environment = new Dictionary<string, string>();
        }

        public ArgsCaller(string program)
        {
            Program = program;
            Arguments = new Dictionary<string, string>();
            Environment = new Dictionary<string, string>();
        }

        public int Run()
        {
            foreach (var key in Arguments.Keys)
                if (Path.IsPathFullyQualified(Arguments[key]))
                    Arguments[key] = PathHelper.RootPath(Arguments[key]);
            StringBuilder sb = new StringBuilder("");
            foreach (var key in Arguments.Keys)
                sb.Append($"{key} {Arguments[key]} ");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Program,
                    Arguments = sb.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
            foreach (var key in Environment.Keys)
                process.StartInfo.Environment.Add(key, Environment[key]);
            if (StdErr != null)
            {
                process.StartInfo.RedirectStandardError = true;
                process.ErrorDataReceived += StdErr;
            }
            if (StdOut != null)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.OutputDataReceived += StdOut;
            }

            process.Start();
            if (StdErr != null)
                process.BeginErrorReadLine();

            if (StdOut != null)
                process.BeginOutputReadLine();

            process.WaitForExit();
            return process.ExitCode;
        }
    }
}
