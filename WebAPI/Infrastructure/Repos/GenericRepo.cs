using Application.IRepos;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Net;

namespace Infrastructure.Repos
{
    public class GenericRepo<TModel> : IGenericRepo<TModel> where TModel : BaseEntity
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

        public async Task<TModel> GetByIdAsync(int id)
        {
            TModel? model = await _dbSet.FindAsync(id);
            if (model == null || model.IsDeleted == true)
            {
                throw new Exceptions.InfrastructureException(HttpStatusCode.BadRequest, $"{model} not found");
            }
            return model;
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

        public async Task<TModel> FindOneAsync(Expression<Func<TModel, bool>> predicate, string includeProperties = "")
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

        public async Task UpdateAsync(TModel model)
        {
            if (model == null || model.IsDeleted== true)
            {
                throw new Exceptions.InfrastructureException(HttpStatusCode.BadRequest, "Data is not exist");
            }

            model.UpdateDate = DateTime.UtcNow;
            _dbSet.Update(model);
            await Task.CompletedTask;
        }
    }
}
