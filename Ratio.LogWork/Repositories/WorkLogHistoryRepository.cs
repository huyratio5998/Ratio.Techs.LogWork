using Ratio.LogWork.Context;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Repositories
{
    public class WorkLogHistoryRepository : GenericRepository<WorkLogHistory>, IWorkLogHistoryRepository
    {
        public WorkLogHistoryRepository(WorkLogDBContext context) : base(context) { }

        public async Task<IEnumerable<WorkLogHistory>> GetHistoriesByWorkLogIdAsync(int workLogId)
        {
            return await FindAsync(h => h.WorkLogId == workLogId);
        }
    }
}
