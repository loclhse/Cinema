using Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IGenericRepo<TModel> where TModel : BaseEntity<Guid>
    {
        Task AddAsync(TModel model);
        void Update(TModel model);
        void Delete(TModel model);
        void SoftDelete(TModel model);
        Task<IEnumerable<TModel>> GetAllAsync();
        Task<TModel> GetByIdAsync(Guid id, CancellationToken ct = default);
        IQueryable<TModel> GetAllQueryable(string includeProperties = "");
        Task<TModel?> FindOneAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "");
    }
}
