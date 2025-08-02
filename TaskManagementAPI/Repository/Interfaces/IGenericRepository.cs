using System.Linq.Expressions;

namespace TaskManagementAPI.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync(int page = 1, int pageSize = 50);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, int page = 1, int pageSize = 50);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    }
}
