using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace PlanSampleGenerator
{
    public class FastDownwardPlanFetcher : IPlanFetcher
    {
        public string OutputPath { get; set; }
        public string PythonPrefix { get; set; }
        public string FastDownwardPath { get; set; }
        // Can either be a "--search" or "--alias"
        public string FastDownwardSearch { get; set; }
        public bool CopyProblemAndDomains { get; set; }

        public FastDownwardPlanFetcher(string outputPath, string pythonPrefix, string fastDownwardPath, string fastDownwardSearch, bool copyProblemAndDomains)
        {
            OutputPath = outputPath;
            PythonPrefix = pythonPrefix;
            FastDownwardPath = fastDownwardPath;
            FastDownwardSearch = fastDownwardSearch;
            CopyProblemAndDomains = copyProblemAndDomains;
        }

        public void Fetch(string domain, List<string> problems, bool multithreaded = true)
        {
            if (Directory.Exists(OutputPath))
                Directory.Delete(OutputPath, true);
            Directory.CreateDirectory(OutputPath);
            Directory.CreateDirectory(Path.Combine(OutputPath, "Plans"));

            var projectPath = ProjectHelper.GetProjectPath();

            List<Task> tasks = new List<Task>();
            foreach (var sub in problems)
                tasks.Add(SampleDomainProblemCombinationAsync(domain, sub, projectPath));
            foreach (var task in tasks)
            {
                task.Start();
                if (!multithreaded)
                    task.Wait();
            }
            Task.WaitAll(tasks.ToArray());

            if (CopyProblemAndDomains)
            {
                File.Copy(Path.Combine(projectPath, domain), Path.Combine(OutputPath, "domain.pddl"));
                foreach (var sub in problems)
                    File.Copy(Path.Combine(projectPath, sub), Path.Combine(OutputPath, new FileInfo(sub).Name));
            }
        }

        private Task SampleDomainProblemCombinationAsync(string domain, string problem, string projectPath)
        {
            return new Task(() =>
            {
                StringBuilder sb = new StringBuilder("");
                sb.Append($"{FastDownwardPath} ");
                string fileName = $"{new FileInfo(domain).Name.Replace(".pddl", "")}-{new FileInfo(problem).Name.Replace(".pddl", "")}";
                sb.Append($"--plan-file \"{Path.Combine(OutputPath, "Plans", fileName)}.plan\" ");
                sb.Append($"--sas-file \"{Path.Combine(OutputPath, "Plans", fileName)}.sas\" ");

                if (FastDownwardSearch.StartsWith("--alias"))
                    sb.Append($"--alias \"lama-first\" ");
                sb.Append($"\"{Path.Combine(projectPath, domain)}\" ");
                sb.Append($"\"{Path.Combine(projectPath, problem)}\" ");
                if (FastDownwardSearch.StartsWith("--search"))
                    sb.Append($"--search \"{FastDownwardSearch}\"");

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = PythonPrefix,
                        Arguments = sb.ToString(),
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        //RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        WorkingDirectory = projectPath
                    }
                };
                //process.OutputDataReceived += (sender, e) =>
                //{
                //    Console.WriteLine(e.Data);
                //};
                process.ErrorDataReceived += (sender, e) =>
                {
                    Console.WriteLine(e.Data);
                };
                process.Start();
                process.BeginErrorReadLine();
                //process.BeginOutputReadLine();
                process.WaitForExit();
            });
        }
    }
}
