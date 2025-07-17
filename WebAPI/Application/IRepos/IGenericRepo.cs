using Domain;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

public interface IGenericRepo<T> where T : class
{
 
    Task<T> GetAsync(Expression<Func<T, bool>> filter);

    Task AddAsync(T entity);

    Task RemoveByIdAsync(object id);

    Task RemoveRangeAsync(IEnumerable<T> entities);

    Task<int> CountAsync();

    Task AddRangeAsync(List<T> entities);

    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter);

    Task<T> GetAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include);

    Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter,
                                           Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, int pageIndex = 1, int pageSize = 25);

    Task<List<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter,
                                               Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null);

    Task UpdateAsync(T entity);

    

    Task<T> GetByIdAsync(Guid id); 
    Task DeleteAsync(Guid id);
}
