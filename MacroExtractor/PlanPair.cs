using PDDLSharp.ErrorListeners;
using PDDLSharp.Models.Plans;
using PDDLSharp.Parsers;
using PDDLSharp.Parsers.Plans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacroExtractor
{
    public class PlanPair
    {
        public FileInfo LeaderPlanFile { get; set; }
        public ActionPlan LeaderPlan { get; set; }
        public FileInfo FollowerPlanFile { get; set; }
        public ActionPlan FollowerPlan { get; set; }

        public PlanPair(FileInfo leaderPlanFile, FileInfo followerPlanFile)
        {
            LeaderPlanFile = leaderPlanFile;
            FollowerPlanFile = followerPlanFile;

            IErrorListener listener = new ErrorListener();
            IParser<ActionPlan> parser = new FastDownwardPlanParser(listener);
            LeaderPlan = parser.Parse(leaderPlanFile);
            FollowerPlan = parser.Parse(followerPlanFile);
        }
    }
}
