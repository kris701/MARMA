using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaActions.Train
{
    public class RunReport
    {
        public string TaskID { get; set; }
        public int TotalMetaActions { get; set; }
        public int TotalValidMetaActions { get; set; }
        public int TotalUsefulMetaActions { get; set; }

        public RunReport(string taskID, int totalMetaActions, int totalValidMetaActions, int totalUsefulMetaActions)
        {
            TaskID = taskID;
            TotalMetaActions = totalMetaActions;
            TotalValidMetaActions = totalValidMetaActions;
            TotalUsefulMetaActions = totalUsefulMetaActions;
        }
    }
}
