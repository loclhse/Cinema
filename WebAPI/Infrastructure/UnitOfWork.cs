using Application;
using Application.IRepos;
using Infrastructure.Repos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;

        public IUserRepo UserRepo { get; }

        public IAuthRepo AuthRepo { get; }
        public IOtpValidRepo OtpValidRepo { get; }

        public UnitOfWork(AppDbContext context, IUserRepo userRepo,
            IAuthRepo authRepo,
            IOtpValidRepo otpValidRepo)
        {
            _context = context;
            UserRepo = userRepo;
            AuthRepo = authRepo;
            OtpValidRepo = otpValidRepo;
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
