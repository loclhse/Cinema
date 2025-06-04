using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application
{
    public interface IUnitOfWork
    {
        IUserRepo Users { get; }
        IAuthRepo Auth { get; }

        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
