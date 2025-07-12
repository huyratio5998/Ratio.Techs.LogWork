using Microsoft.EntityFrameworkCore;
using Ratio.LogWork.Context;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Repositories
{
    public class WorkingProjectRepository : GenericRepository<WorkingProject>, IWorkingProjectRepository
    {
        public WorkingProjectRepository(WorkLogDBContext context) : base(context) { }

        public async Task<WorkingProject?> GetActiveProjectAsync()
        {
            return await GetAll().FirstOrDefaultAsync(p => p.ProjectStatus == WorkingProjectStatus.Active);
        }
    }
}
