namespace MetaActions.Train
{
    public class RunReport
    {
        public string TaskID { get; set; }
        public int TotalTrainingProblems { get; set; }
        public int TotalTestingProblems { get; set; }
        public int TotalMetaActions { get; set; }
        public int TotalValidMetaActions { get; set; }
        public int TotalReplacementMacros { get; set; }

        public RunReport(string taskID, int totalTrainingProblems, int totalTestingProblems, int totalMetaActions, int totalValidMetaActions, int totalReplacementMacros)
        {
            TaskID = taskID;
            TotalTrainingProblems = totalTrainingProblems;
            TotalTestingProblems = totalTestingProblems;
            TotalMetaActions = totalMetaActions;
            TotalValidMetaActions = totalValidMetaActions;
            TotalReplacementMacros = totalReplacementMacros;
        }
    }
}
