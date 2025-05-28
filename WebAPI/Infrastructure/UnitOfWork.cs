using Application;
using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;
        public readonly IUserRepo _userRepo;

        public IUserRepo UserRepo => _userRepo;

        public UnitOfWork(AppDbContext context, IUserRepo userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }


    }
}
