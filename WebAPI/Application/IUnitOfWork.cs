using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application
{
    public interface IUnitOfWork
    {
        IUserRepo UserRepo { get; }
        
        IAuthRepo AuthRepo { get; }
        
        IOtpValidRepo OtpValidRepo { get; }
        IPromotionRepo PromotionRepo { get; }
        IMovieRepo MovieRepo { get; }
        IGenreRepo GenreRepo { get; }

        IMovieRepo MovieRepo { get; }
        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
