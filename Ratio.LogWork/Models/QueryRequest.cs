namespace Ratio.LogWork.Models
{
    public class QueryRequest
    {
        public string Command { get; set; }
        public string TicketId { get; set; }
        public string Description { get; set; }
        public string ProjectName { get; set; }
        public string RawQuery { get; set; }
    }
}
