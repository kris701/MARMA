using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Tools
{
    public class ArgsCaller
    {
        public event DataReceivedEventHandler? StdErr;
        public event DataReceivedEventHandler? StdOut;

        public string WorkingDirectory { get; set; } = "temp";
        public string Program { get; }
        public Dictionary<string, string> Arguments { get; }

        public ArgsCaller(string program, Dictionary<string, string> arguments)
        {
            Program = PathHelper.RootPath(program);
            Arguments = arguments;
        }

        public ArgsCaller(string program)
        {
            Program = PathHelper.RootPath(program);
            Arguments = new Dictionary<string, string>();
        }

        public void Run()
        {
            foreach (var key in Arguments.Keys)
                if (Path.IsPathFullyQualified(Arguments[key]))
                    Arguments[key] = PathHelper.RootPath(Arguments[key]);
            StringBuilder sb = new StringBuilder("");
            foreach (var key in Arguments.Keys)
                sb.Append($"{key} \"{Arguments[key]}\" ");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Program,
                    Arguments = sb.ToString(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WorkingDirectory = WorkingDirectory
                }
            };
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
        }
    }
}
