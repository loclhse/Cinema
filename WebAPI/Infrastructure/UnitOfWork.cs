using Application;
using Application.IRepos;
using Application.IServices;
using Elastic.Clients.Elasticsearch;
using Infrastructure.Identity;
using Infrastructure.Repos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;
       
        public ISnackRepo SnackRepo { get; }
        public IUserRepo UserRepo { get; }
        public IAuthRepo AuthRepo { get; }
        public IMovieRepo MovieRepo { get; }
        public IOtpValidRepo OtpValidRepo { get; }
        public ICinemaRoomRepo CinemaRoomRepo { get; }
        public IRoomLayoutRepo RoomLayoutRepo { get; }
        public ISeatRepo SeatRepo { get; }
        public ISeatTypePriceRepo SeatTypePriceRepo { get; }
        public IPaymentRepo PaymentRepo { get; }
        public IGenreRepo GenreRepo { get; }
        public IPromotionRepo PromotionRepo { get; }
        public IShowtimeRepo ShowtimeRepo { get; }
        public IMovieGenreRepo MovieGenreRepo { get; }
        public ISeatScheduleRepo SeatScheduleRepo { get; }
        public ISubscriptionPlanRepo SubscriptionPlanRepo { get; }
        public IScoreItemRepo ScoreItemRepo { get; }

        public ISnackComboRepo SnackComboRepo { get; }
        public ISubscriptionRepo SubscriptionRepo { get; }

        public IOrderRepo OrderRepo { get; }
        public IElasticMovieRepo elasticMovieRepo { get; }
        public UnitOfWork(AppDbContext context, ElasticsearchClient elasticClient, UserManager<ApplicationUser> userManager, ILogger<AuthRepo> logger, IUserRepo userRepo,
            IAuthRepo authRepo,
            IOtpValidRepo otpValidRepo)
        {
            _context = context;
            UserRepo = userRepo;
            AuthRepo = authRepo;
            OtpValidRepo = otpValidRepo;
            CinemaRoomRepo = new CinemaRoomRepo(context);
            SeatRepo = new SeatRepo(context);
            SeatTypePriceRepo = new SeatTypePriceRepo(context);
            PaymentRepo = new PaymentRepo(context);
            MovieRepo = new MovieRepo(context);
            GenreRepo = new GenreRepo(context);
            PromotionRepo = new PromotionRepo(context);
            RoomLayoutRepo = new RoomLayoutRepo(context);
            
            SnackRepo = new SnackRepo(context);
            ShowtimeRepo = new ShowtimeRepo(context);
            MovieGenreRepo = new MovieGenreRepo(context);
            SeatScheduleRepo = new SeatScheduleRepo(context);
            SubscriptionPlanRepo = new SubscriptionPlanRepo(context);
            SnackComboRepo = new SnackComboRepo(context);
            SubscriptionRepo = new SubscriptionRepo(context);
            OrderRepo = new OrderRepo(context);
            ScoreItemRepo = new ScoreItemRepo(context);
            elasticMovieRepo = new ElasticMovieRepo(elasticClient);
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
