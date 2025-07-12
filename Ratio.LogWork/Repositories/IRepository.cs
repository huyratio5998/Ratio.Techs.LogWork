using Microsoft.EntityFrameworkCore;
using Ratio.LogWork.Entity;
using System.Linq.Expressions;

namespace Ratio.LogWork.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        // Read operations
        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAll();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Create operations
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        
        // Update operations
        Task UpdateAsync(T entity);
        
        // Delete operations
        Task RemoveAsync(T entity);
        Task RemoveRangeAsync(IEnumerable<T> entities);               
    }
}
