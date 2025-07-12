using Ratio.LogWork.Context;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Repositories
{
    public class WorkLogRepository : GenericRepository<WorkLog>, IWorkLogRepository
    {
        public WorkLogRepository(WorkLogDBContext context) : base(context) { }

        public async Task<IEnumerable<WorkLog>> GetWorkLogsByProjectAsync(int projectId)
        {
            return await FindAsync(w => w.WorkingProjectId == projectId);
        }

        public async Task<IEnumerable<WorkLog>> GetActiveWorkLogsAsync()
        {
            return await FindAsync(w => w.Status == WorkLogStatus.Active);
        }
    }
}
