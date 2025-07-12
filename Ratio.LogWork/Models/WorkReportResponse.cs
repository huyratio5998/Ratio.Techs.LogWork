namespace Ratio.LogWork.Models
{
    public class WorkReportResponse
    {
        public DateTime ReportDate { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public double TotalWorkingHours { get; set; }
    }
}
