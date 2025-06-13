using Application.IRepos;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application
{
    public interface IUnitOfWork
    {
        IUserRepo UserRepo { get; }
        
        IAuthRepo AuthRepo { get; }
        
        IOtpValidRepo OtpValidRepo { get; }
        ICinemaRoomRepo CinemaRoomRepo { get; }
        ISeatRepo SeatRepo { get; }
        ISeatTypePriceRepo SeatTypeConfigRepo { get; }

        IPromotionRepo PromotionRepo { get; }
        IMovieRepo MovieRepo { get; }
        IGenreRepo GenreRepo { get; }
        IShowtimeRepo ShowtimeRepo { get; }

       
        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
