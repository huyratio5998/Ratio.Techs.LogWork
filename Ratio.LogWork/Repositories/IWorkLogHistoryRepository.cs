using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Repositories
{
    public interface IWorkLogHistoryRepository : IRepository<WorkLogHistory>
    {
        Task<IEnumerable<WorkLogHistory>> GetHistoriesByWorkLogIdAsync(int workLogId);
    }
}
