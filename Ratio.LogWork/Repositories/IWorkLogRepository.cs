using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Repositories
{
    public interface IWorkLogRepository : IRepository<WorkLog>
    {
        Task<IEnumerable<WorkLog>> GetWorkLogsByProjectAsync(int projectId);
        Task<IEnumerable<WorkLog>> GetActiveWorkLogsAsync();
    }
}
