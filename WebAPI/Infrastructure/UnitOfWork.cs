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
        public ISnackComboRepo SnackComboRepo { get; }
        public ISnackRepo SnackRepo { get; }
        public IUserRepo UserRepo { get; }
        public IAuthRepo AuthRepo { get; }
        public IMovieRepo MovieRepo { get; }
        public IOtpValidRepo OtpValidRepo { get; }
        public ICinemaRoomRepo CinemaRoomRepo { get; }
        public IRoomLayoutRepo RoomLayoutRepo { get; }
        public ISeatRepo SeatRepo { get; }
        public ISeatTypePriceRepo SeatTypePriceRepo { get; }
        public IGenreRepo GenreRepo { get; }
        public IPromotionRepo PromotionRepo { get; }
        public IShowtimeRepo ShowtimeRepo { get; }
        public IMovieGenreRepo MovieGenreRepo { get; }
        public UnitOfWork(AppDbContext context, UserManager<ApplicationUser> userManager, ILogger<AuthRepo> logger, IUserRepo userRepo,
            IAuthRepo authRepo,
            IOtpValidRepo otpValidRepo,
            IMovieRepo movieRepo, IMovieGenreRepo movieGenreRepo)
        {
            _context = context;
            UserRepo = userRepo;
            AuthRepo = authRepo;
            OtpValidRepo = otpValidRepo;
            CinemaRoomRepo = new CinemaRoomRepo(context);
            SeatRepo = new SeatRepo(context);
            SeatTypePriceRepo = new SeatTypePriceRepo(context);
            MovieRepo = movieRepo;
            GenreRepo = new GenreRepo(context);
            PromotionRepo = new PromotionRepo(context);
            RoomLayoutRepo = new RoomLayoutRepo(context);
            SnackComboRepo = new SnackComboRepo(context);
            SnackRepo = new SnackRepo(context);
            ShowtimeRepo = new ShowtimeRepo(context);
            MovieGenreRepo = movieGenreRepo;
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
