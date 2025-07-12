using Ratio.LogWork.Entity;
using Ratio.LogWork.Repository;

namespace Ratio.LogWork.Repositories
{
    public interface IWorkingProjectRepository : IRepository<WorkingProject>
    {
        Task<WorkingProject?> GetActiveProjectAsync();
    }
}
