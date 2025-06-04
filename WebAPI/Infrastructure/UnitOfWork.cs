using Application;
using Application.IRepos;
using Infrastructure.Repos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;

        public IUserRepo Users { get; }

        public IAuthRepo Auth { get; }

        public UnitOfWork(AppDbContext context, IUserRepo userRepo,
            IAuthRepo authRepo)
        {
            _context = context;
            Users = userRepo;
            Auth = authRepo;

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
