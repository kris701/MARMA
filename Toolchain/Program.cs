using DependencyFetcher;
using Tools;

namespace Toolchain
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Toolchain started!");
            var projectFolder = ProjectHelper.GetProjectPath();

            CheckDependencies(projectFolder);
        }

        private static void CheckDependencies(string projectFolder)
        {
            var dependenciesFile = Path.Combine(projectFolder, "Dependencies", "dependencies.json");
            IDependencyChecker checker = new DependencyChecker(dependenciesFile);
            checker.CheckDependencies();
        }
    }
}