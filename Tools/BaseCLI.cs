﻿using CommandLine;
using CommandLine.Text;

namespace Tools
{
    public abstract class BaseCLI
    {
        public static void HandleParseError(IEnumerable<Error> errs)
        {
            var sentenceBuilder = SentenceBuilder.Create();
            foreach (var error in errs)
                if (error is not HelpRequestedError)
                    ConsoleHelper.WriteLineColor(sentenceBuilder.FormatError(error), ConsoleColor.Red);
        }
    }
}
