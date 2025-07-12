using Microsoft.EntityFrameworkCore;
using Ratio.LogWork.Context;
using Ratio.LogWork.Entity;
using System.Linq.Expressions;

namespace Ratio.LogWork.Repository
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly WorkLogDBContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(WorkLogDBContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public IQueryable<T> GetAll() => _dbSet;

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
        {
            if (entity == null) return;

            if (entity.CreatedDate == DateTime.MinValue)
            {
                entity.CreatedDate = DateTime.UtcNow;
            }

            await _dbSet.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            await Task.CompletedTask;
        }

    }
}
