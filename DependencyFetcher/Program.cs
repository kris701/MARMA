using System;

namespace DependencyFetcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IDependencyChecker checker = new DependencyChecker(args[0]);
            checker.CheckDependencies();
        }
    }
}