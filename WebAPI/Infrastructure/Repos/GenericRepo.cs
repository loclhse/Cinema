using Application.IRepos;
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
    }
}