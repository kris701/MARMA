using DependencyFetcher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tools;

namespace DependencyFetcher
{
    public class DependencyChecker : IDependencyChecker
    {
        public DependencyList Dependencies { get; internal set; }

        public DependencyChecker(string dependencyFile)
        {
            if (!File.Exists(dependencyFile))
                throw new IOException($"File not found: {dependencyFile}");

            var parsed = JsonSerializer.Deserialize<DependencyList>(File.ReadAllText(dependencyFile));
            if (parsed == null)
                throw new ArgumentNullException("Deserialized dependency list was empty!");
            Dependencies = parsed;
        }

        public void CheckDependencies(string root)
        {
            foreach(var dependency in Dependencies.Dependencies)
            {
                Console.WriteLine($"Checking dependency: {dependency.Name}");

                if (!Path.IsPathRooted(root))
                    root = Path.Join(Directory.GetCurrentDirectory(), root);

                var targetFolder = Path.Join(root, dependency.TargetLocation);
                if (!Directory.Exists(targetFolder))
                {
                    Console.WriteLine($"Dependency '{dependency.Name}' is not installed!");
                    Console.WriteLine($"Do you wish to automatically fetch and install it to '{dependency.TargetLocation}'? (y/n)");
                    var response = GetYNResponse();
                    if (response)
                    {
                        Directory.CreateDirectory(targetFolder);
                        foreach(var call in dependency.SetupCalls)
                        {
                            Console.WriteLine($"Step '{call.Name}' started...");
                            if (call.Command.ToLower() == "cd")
                                targetFolder = Path.Join(targetFolder, call.Arguments);
                            else
                                ExecuteCall(targetFolder, call.Command, call.Arguments);
                            Console.WriteLine($"Step '{call.Name}' finished!");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ignoring dependency '{dependency.Name}'...");
                    }

                } else
                {
                    Console.WriteLine($"Dependency '{dependency.Name}' is installed!");
                }
            }
        }

        private bool GetYNResponse()
        {
            //return true;
            char key = ' ';
            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                key = Console.ReadKey().KeyChar;

            if (key == 'y' || key == 'Y')
                return true;
            return false;
        }

        private void ExecuteCall(string folder, string command, string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = command,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = folder
                }
            };
            process.OutputDataReceived += RecieveOutputData;
            process.ErrorDataReceived += RecieveErrorData;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private void RecieveErrorData(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"[ERRO] {e.Data}");
        }

        private void RecieveOutputData(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"[INFO] {e.Data}");
        }
    }
}
