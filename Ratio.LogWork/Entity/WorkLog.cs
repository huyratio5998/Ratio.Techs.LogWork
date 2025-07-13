namespace Ratio.LogWork.Entity
{
    public class WorkLog : BaseEntity
    {
        public string Command { get; set; }
        public string? Name { get; set; }
        public string FullCommand { get; set; }
        public string? TaskID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double WorkingHour { get; set; }
        public WorkLogType WorkType { get; set; }
        public WorkLogStatus Status { get; set; }

        public int WorkingProjectId { get; set; }
        public WorkingProject Project { get; set; }
        public ICollection<WorkLogHistory> WorkLogHistories { get; set; }
    }
    
    public enum WorkLogType
    {
        Task = 0,
        Meeting = 1,
        Support = 2,
        Lunch = 3,
        GoHome = 4,
        Event = 5,
        OnLeave = 6,
        Other = 7,

    }

    public enum WorkLogStatus
    {
        Active = 0,
        Paused = 1,
        Done = 2,
        Cancelled = 3
    }
}
