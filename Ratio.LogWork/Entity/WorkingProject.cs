namespace Ratio.LogWork.Entity
{
    public class WorkingProject : BaseEntity
    {
        public string Name { get; set; }
        public WorkingProjectStatus ProjectStatus { get; set; }

        public ICollection<WorkLog> WorkLogs { get; set; }
    }

    public enum WorkingProjectStatus
    {
        Active = 0,
        None = 1,        
    }
}
