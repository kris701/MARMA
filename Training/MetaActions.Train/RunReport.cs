namespace MetaActions.Train
{
    public class RunReport
    {
        public string TaskID { get; set; }
        public int TotalTrainingProblems { get; set; }
        public int TotalTestingProblems { get; set; }
        public int TotalMacros { get; set; }
        public int TotalMetaActions { get; set; }
        public int TotalValidMetaActions { get; set; }
        public int TotalValidMetaActionsTimedOut { get; set; }
        public int TotalCheckedMetaActions { get; set; }
        public int TotalReplacementMacros { get; set; }
        public bool TimedOut { get; set; }
        public long EllapsedSeconds { get; set; }

        public RunReport(string taskID, int totalTrainingProblems, int totalTestingProblems, int totalMacros, int totalMetaActions, int totalValidMetaActions, int totalValidMetaActionsTimedOut, int totalCheckedMetaActions, int totalReplacementMacros, bool timedOut, long ellapsedSeconds)
        {
            TaskID = taskID;
            TotalTrainingProblems = totalTrainingProblems;
            TotalTestingProblems = totalTestingProblems;
            TotalMacros = totalMacros;
            TotalMetaActions = totalMetaActions;
            TotalValidMetaActions = totalValidMetaActions;
            TotalValidMetaActionsTimedOut = totalValidMetaActionsTimedOut;
            TotalCheckedMetaActions = totalCheckedMetaActions;
            TotalReplacementMacros = totalReplacementMacros;
            TimedOut = timedOut;
            EllapsedSeconds = ellapsedSeconds;
        }
    }
}
