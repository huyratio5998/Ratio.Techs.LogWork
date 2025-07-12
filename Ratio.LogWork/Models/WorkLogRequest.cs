using Ratio.LogWork.Entity;

namespace Ratio.LogWork.Models
{
    public class WorkLogRequest : RatioCommand
    {
        public WorkLogType Action { get; set; }
        public string TaskID { get; set; }
        public string Name { get; set; }

        public int? WorkingProjectId { get; set; }
    }

    public class RatioCommand
    {
        public RatioCommandType CommandType { get; set; }
        public string Command { get; set; }
        public string FullCommand { get; set; }
    }

    public enum RatioCommandType
    {
        Project = 0,
        Task = 1
    }
}
