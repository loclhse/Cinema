using Application.IRepos;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GenericRepo<T> : IGenericRepo<T> where T : class
    {
        public readonly DbSet<T> _db;
        public readonly AppDbContext _context;

        //Kiet
        public GenericRepo(AppDbContext context)
        {
            _context = context;
            _db = _context.Set<T>();
        }
        //Kiet
        public async Task AddAsync(T entity)
        {
            await _db.AddAsync(entity);
        }
        //Kiet
        public async Task<int> CountAsync() => await _db.CountAsync();
        //Kiet
        public async Task<List<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter,
                                               Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null,
                                               int pageIndex = 1,
                                               int pageSize = 5)
        {
            IQueryable<T> query = _db;


            if (filter != null)
            {
                query = query.Where(filter);
            }

            //query.IgnoreQueryFilters();

            if (include != null)
            {
                query = include(query);
            }
            return await query
                //.Skip((pageIndex - 1) * pageSize)
                //.Take(pageSize)
                .ToListAsync();
        }
        //Kiet
        public async Task<List<T>> GetAllAsync(System.Linq.Expressions.Expression<Func<T, bool>>? filter)
        {
            if (filter != null)
            {
                return await _db.Where(filter).ToListAsync();
            }
            return await _db.ToListAsync();
        }
        //Kiet
        public async Task<T> GetAsync(System.Linq.Expressions.Expression<Func<T, bool>> filter)
        {
#nullable disable
            IQueryable<T> query = _db;
            return await query.FirstOrDefaultAsync(filter);
#nullable restore
        }
        //Kiet
        public async Task<T> GetAsync(System.Linq.Expressions.Expression<Func<T, bool>> filter, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null)
        {

            IQueryable<T> query = _db;
            if (include != null)
            {
                query = include(query);
            }
#pragma warning disable CS8603 // Possible null reference return.
            return await query.FirstOrDefaultAsync(filter);
#pragma warning restore CS8603 // Possible null reference return.

        }
        //Kiet
        public async Task RemoveByIdAsync(object id)
        {
#nullable disable
            T existing = await _db.FindAsync(id);
#nullable restore
            if (existing != null)
            {
                _db.Remove(existing);
            }
            else throw new Exception();
        }
        //Kiet
        public async Task AddRangeAsync(List<T> entities)
        {
            await _db.AddRangeAsync(entities);
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _db.Update(entity);
            await _context.SaveChangesAsync(); // Ensure changes are saved to the database
        }

        public async Task RemoveRangeAsync(IEnumerable<T> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (!entityList.Any()) return;

            // (Optional) Verify that all entities are currently tracked or exist in the database:
            // var keys = entityList.Select(e => EF.Property<object>(e, "Id")).ToList();
            // var existingCount = await _db.Where(e => keys.Contains(EF.Property<object>(e, "Id"))).CountAsync();
            // if (existingCount != entityList.Count)
            //     throw new InvalidOperationException("One or more entities were not found in the database.");

            _db.RemoveRange(entityList);
            await _context.SaveChangesAsync();
        }


        public async Task<T> GetByIdAsync(Guid id)
        {

#pragma warning disable CS8603 // Possible null reference return.
            return await _db.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
#pragma warning restore CS8603 // Possible null reference return.
        }


       

            public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {id} not found.");
            }
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.UpdateDate = DateTime.UtcNow;
                _db.Update(entity);
                await _context.SaveChangesAsync();
            }
            else
            {
                _db.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
