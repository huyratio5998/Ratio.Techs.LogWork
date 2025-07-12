namespace Ratio.LogWork.Models
{
    public class BaseWorkLogResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class WorkLogResponse : BaseWorkLogResponse
    {
        public string Title { get; set; }
        public List<WorkItemResponse> Actions { get; set; }        
    }

    public class WorkItemResponse
    {
        public string Name { get; set; }
        public double WorkingTime { get; set; }
    }
}
