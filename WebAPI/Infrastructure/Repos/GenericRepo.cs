using Domain;
using Infrastructure;
using Application.IRepos;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;
using Infrastructure.Exceptions;

namespace Infrastructure.Repos
{
    public class GenericRepo<TModel> : IGenericRepo<TModel> where TModel : BaseEntity<Guid>
    {
        protected DbSet<TModel> _dbSet;

        public GenericRepo(AppDbContext dbContext)
        {
            _dbSet = dbContext.Set<TModel>();
        }

        public async Task AddAsync(TModel model)
        {
            await _dbSet.AddAsync(model);
        }

        public void Delete(TModel model)
        {
            _dbSet.Remove(model);
        }

        public async Task<IEnumerable<TModel>> GetAllAsync()
        {
            var result = _dbSet;
            foreach (var item in result)
            {
                if (item.IsDeleted)
                {
                    result.Remove(item);
                }
            }
            return await result.ToListAsync();
        }

        public async Task<TModel> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, ct);
            if (entity == null || entity.IsDeleted)
                throw new InfrastructureException(HttpStatusCode.BadRequest, "Data is not exist");
            return entity;
        }

        public void SoftDelete(TModel model)
        {
            model.IsDeleted = true;
            model.UpdateDate = DateTime.UtcNow;
        }

        public void Update(TModel model)
        {
            if (model == null || model.IsDeleted == true)
            {
                throw new Exceptions.InfrastructureException(HttpStatusCode.BadRequest, "Data is not exist");
            }
            model.UpdateDate = DateTime.UtcNow;
            _dbSet.Update(model);
        }

        public virtual IQueryable<TModel> GetAllQueryable(string includeProperties = "")
        {
            IQueryable<TModel> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return query.Where(x => !x.IsDeleted);
        }

        public async Task<TModel?> FindOneAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "")
        {
            IQueryable<TModel> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.Where(x => !x.IsDeleted).FirstOrDefaultAsync(predicate);
        }
    }
}
