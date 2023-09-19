using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Tools.Benchmarks;

namespace Tools
{
    public abstract class BaseCLI
    {
        public static void HandleParseError(IEnumerable<Error> errs)
        {
            var sentenceBuilder = SentenceBuilder.Create();
            foreach (var error in errs)
            {
                ConsoleHelper.WriteLineColor(sentenceBuilder.FormatError(error), ConsoleColor.Red);
            }
        }
    }
}
