using Microsoft.EntityFrameworkCore.Storage;
using Ratio.LogWork.Context;
using Ratio.LogWork.Entity;
using Ratio.LogWork.Repositories;
using System.Collections.Concurrent;

namespace Ratio.LogWork.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly WorkLogDBContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();
        private bool _disposed;

        public IWorkLogRepository WorkLogs { get; }
        public IWorkLogHistoryRepository WorkLogHistories { get; }
        public IWorkingProjectRepository WorkingProjects { get; }

        public UnitOfWork(
            WorkLogDBContext context,
            IWorkLogRepository workLogRepository,
            IWorkLogHistoryRepository workLogHistoryRepository,
            IWorkingProjectRepository workingProjectRepository)
        {
            _context = context;
            WorkLogs = workLogRepository;
            WorkLogHistories = workLogHistoryRepository;
            WorkingProjects = workingProjectRepository;
        }

        public WorkLogDBContext Context => _context;

        public IRepository<T> GetRepository<T>() where T : BaseEntity
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context));
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context?.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }
    }
}