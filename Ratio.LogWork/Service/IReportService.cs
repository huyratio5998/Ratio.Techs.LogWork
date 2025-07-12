using Ratio.LogWork.Models;

namespace Ratio.LogWork.Service
{
    public interface IReportService
    {       
        Task<WorkReportResponse> GetReport(DateTime reportDate);
        Task<WorkReportResponse> GetReports(DateTime from, DateTime to);
    }
}
