using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application
{
    public interface IUnitOfWork:IDisposable
    {
        IUserRepo UserRepo { get; }

        public Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
