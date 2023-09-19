using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Tools
{
    public abstract class BaseCLI<T>
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<T>(args)
              .WithParsed(RunOptions)
              .WithNotParsed(HandleParseError);
        }

        static void HandleParseError(IEnumerable<Error> errs)
        {
            foreach (var error in errs)
                ConsoleHelper.WriteLineColor($"{error}", ConsoleColor.Red);
        }

        public abstract void RunOptions(T opts);
    }
}
