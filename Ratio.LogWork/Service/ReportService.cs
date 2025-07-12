using Ratio.LogWork.Models;

namespace Ratio.LogWork.Service
{
    public class ReportService : IReportService
    {
        public Task<WorkReportResponse> GetReport(DateTime reportDate)
        {
            throw new NotImplementedException();
        }

        public Task<WorkReportResponse> GetReports(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
    }
}
