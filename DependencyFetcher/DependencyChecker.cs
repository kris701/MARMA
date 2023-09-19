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

        public void CheckDependencies()
        {
            var projectFolder = ProjectHelper.GetProjectPath();
            foreach(var dependency in Dependencies.Dependencies)
            {
                Console.WriteLine($"Checking dependency: {dependency.Name}");

                var targetFolder = Path.Join(projectFolder, dependency.TargetLocation);
                if (!Directory.Exists(targetFolder))
                {
                    Console.WriteLine($"Dependency '{dependency.Name}' is not installed!");
                    Console.WriteLine($"Do you wish to automatically fetch and install it to '{dependency.TargetLocation}'? (y/n)");
                    var response = GetYNResponse();
                    if (response)
                    {
                        Console.WriteLine($"Cloning dependency '{dependency.Name}'...");
                        CloneRepository(projectFolder, dependency);
                        if (dependency.BuildCommand != null)
                        {
                            Console.WriteLine($"Building dependency '{dependency.Name}'...");
                            BuildRepository(projectFolder, dependency);
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
            char key = ' ';
            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                key = Console.ReadKey().KeyChar;

            if (key == 'y' || key == 'Y')
                return true;
            return false;
        }

        private void CloneRepository(string projectFolder, Dependency dependency)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "git",
                    Arguments = $"clone {dependency.RepositoryLink} {Path.Join(projectFolder, dependency.TargetLocation)}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = projectFolder
                }
            };
            process.OutputDataReceived += RecieveOutputData;
            process.ErrorDataReceived += RecieveErrorData;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }

        private void BuildRepository(string projectFolder, Dependency dependency)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = dependency.BuildCommand,
                    Arguments = dependency.BuildArgs,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = projectFolder
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
