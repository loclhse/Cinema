using Application;
using Application.IRepos;
using Infrastructure.Identity;
using Infrastructure.Repos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;

        public IUserRepo UserRepo { get; }
        public IAuthRepo AuthRepo { get; }
        public IOtpValidRepo OtpValidRepo { get; }
        public IMovieRepo MovieRepo { get; }
        public ICinemaRoomRepo CinemaRoomRepo { get; }
        public ISeatRepo SeatRepo { get; }
        public ISeatTypePriceRepo SeatTypeConfigRepo { get; }
        public IGenreRepo GenreRepo { get; }
        public IPromotionRepo PromotionRepo { get; }
        public IShowtimeRepo ShowtimeRepo { get; }
        public UnitOfWork(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<AuthRepo> logger, IUserRepo userRepo,
            IAuthRepo authRepo,
            IOtpValidRepo otpValidRepo,
            IPromotionRepo promotionRepo,
            IMovieRepo movieRepo, IGenreRepo genre, IShowtimeRepo showtimeRepo)
        {
            _context = context;
            UserRepo = userRepo;
            AuthRepo = authRepo;
            OtpValidRepo = otpValidRepo;
            CinemaRoomRepo = new CinemaRoomRepo(context);
            SeatRepo = new SeatRepo(context);
            SeatTypeConfigRepo = new SeatTypePriceRepo(context);
            PromotionRepo = promotionRepo;
            MovieRepo = movieRepo;
            GenreRepo = genre;
            ShowtimeRepo = showtimeRepo;
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
