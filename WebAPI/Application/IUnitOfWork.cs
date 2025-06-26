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
        IRoomLayoutRepo RoomLayoutRepo { get; }
        ISeatRepo SeatRepo { get; }
        ISeatTypePriceRepo SeatTypePriceRepo { get; }
      
        ISnackRepo SnackRepo { get; }
        IMovieRepo MovieRepo { get; }
        IPromotionRepo PromotionRepo { get; }
        IGenreRepo GenreRepo { get; }
        IShowtimeRepo ShowtimeRepo { get; }
        IMovieGenreRepo MovieGenreRepo { get; }
        ISeatScheduleRepo SeatScheduleRepo { get; }
        ISubscriptionPlanRepo SubscriptionPlanRepo { get; }
        ISnackComboRepo SnackComboRepo { get; }
        Task<int> SaveChangesAsync();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
