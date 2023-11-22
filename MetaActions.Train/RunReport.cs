namespace MetaActions.Train
{
    public class RunReport
    {
        public string TaskID { get; set; }
        public int TotalMetaActions { get; set; }
        public int TotalValidMetaActions { get; set; }

        public RunReport(string taskID, int totalMetaActions, int totalValidMetaActions)
        {
            TaskID = taskID;
            TotalMetaActions = totalMetaActions;
            TotalValidMetaActions = totalValidMetaActions;
        }
    }
}
