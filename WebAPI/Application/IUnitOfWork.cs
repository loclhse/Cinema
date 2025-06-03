using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application
{
    public interface IUnitOfWork
    {
        IUserRepo Users { get; }
        IAuthRepo Auth { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    }
}
