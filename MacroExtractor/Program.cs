using CommandLine;
using System;
using Tools;

namespace MacroExtractor
{
    internal class Program : BaseCLI
    {
        static int Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(ExtractMacros)
              .WithNotParsed(HandleParseError);
            return 0;
        }

        public static void ExtractMacros(Options opts)
        {
            ConsoleHelper.WriteLineColor("");

            List<FileInfo> leaderPlans = new List<FileInfo>();
            foreach (var plan in opts.LeaderPlans)
                leaderPlans.Add(new FileInfo(PathHelper.RootPath(plan)));
            List<FileInfo> followerPlans = new List<FileInfo>();
            foreach (var plan in opts.FollowerPlans)
                followerPlans.Add(new FileInfo(PathHelper.RootPath(plan)));

            List<PlanPair> planPairs = new List<PlanPair>();
            foreach(var leaderPlan in leaderPlans)
            {
                var matches = followerPlans.Where(x => x.Name == leaderPlan.Name);
                foreach (var match in matches)
                    planPairs.Add(new PlanPair(leaderPlan, match));
            }


        }
    }
}