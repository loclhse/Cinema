using Domain;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Application.IRepos
{
    public interface IGenericRepo<T> where T : class
    {
        //Kiet  
        Task<T> GetAsync(Expression<Func<T, bool>> filter);
        //Kiet
        Task AddAsync(T entity);
        //Kiet
        Task RemoveByIdAsync(object id);
        //Kiet
        Task<int> CountAsync();
        //Kiet
        Task AddRangeAsync(List<T> entities);
        //Kiet
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter);
        //Kiet
        Task<T> GetAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include);
        //Kiet
        Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter,
                                               Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, int pageIndex = 1, int pageSize = 25);
    }
}
