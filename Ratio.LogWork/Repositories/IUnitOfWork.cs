using Ratio.LogWork.Context;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repositories;

namespace Ratio.LogWork.Repository
{
    public interface IUnitOfWork : IDisposable
    {        
        IRepository<T> GetRepository<T>() where T : BaseEntity;

        IWorkLogRepository WorkLogs { get; }
        IWorkLogHistoryRepository WorkLogHistories { get; }
        IWorkingProjectRepository WorkingProjects { get; }
        WorkLogDBContext Context { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task SaveChangesAsync();
    }

}
