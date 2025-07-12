namespace Ratio.LogWork.Entity
{
    public class WorkLogHistory : BaseEntity
    {
        public WorkLogHistoryAction Action { get; set; }        

        public int WorkLogId { get; set; }
        public WorkLog WorkLogEntity { get; set; }
    }

    public enum WorkLogHistoryAction
    {
        Start = 0,
        Pause = 1,
        Continue = 2,
        Done = 3,
        Cancel = 4,
        NoAction = 5 // Use to log when no action is taken but still want to record in history
    }
}
