using Application;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Hangfire;
using Hangfire.PostgreSql;
using Infrastructure.Configuration;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Repos;
using Infrastructure.Service;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using WebAPI.Infrastructure.Services;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Đăng ký AppDbContext (PostgreSQL)
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("Local"),
                    npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            });

            //Bind JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

            // 2. Cấu hình ASP.NET Identity với ApplicationUser là IdentityUser<Guid>
            services.AddIdentity<ApplicationUser, AppRole>(options =>
            {
                // Ví dụ: chỉnh policy mật khẩu
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            services.AddHttpContextAccessor();

            #region Repositories
            // 3. Đăng ký Repositories
            services.AddScoped<IUserRepo, UserRepo>();
            services.AddScoped<IAuthRepo, AuthRepo>();
            services.AddScoped<IOtpValidRepo, OtpValidRepo>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ISeatRepo, SeatRepo>();
            services.AddScoped<ICinemaRoomRepo, CinemaRoomRepo>();
            services.AddScoped<ISeatTypePriceRepo, SeatTypePriceRepo>();
            services.AddScoped<IPromotionRepo, PromotionRepo>();
            services.AddScoped<IRoomLayoutRepo, RoomLayoutRepo>();
            services.AddScoped<ISnackRepo,SnackRepo>();
            services.AddScoped<ISnackComboRepo, SnackComboRepo>();
            services.AddScoped<IPaymentRepo, PaymentRepo>();
            services.AddScoped<IGenreRepo, GenreRepo>();
            services.AddScoped<IMovieRepo, MovieRepo>();
            services.AddScoped<IShowtimeRepo,ShowtimeRepo>();
            services.AddScoped<IMovieGenreRepo, MovieGenreRepo>();
            services.AddScoped<ISeatScheduleRepo, SeatScheduleRepo>();
            services.AddScoped<ISubscriptionPlanRepo, SubscriptionPlanRepo>();
            services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();
            services.AddScoped<IOrderRepo, OrderRepo>();
            services.AddScoped<IElasticMovieRepo, ElasticMovieRepo>();
            services.AddScoped<IScoreItemRepo, ScoreItemRepo>();
            services.AddScoped<ISnackOrderRepo, SnackOrderRepo>();
            // 4. Đăng ký JwtTokenGenerator (sinh JWT)
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            #endregion

            #region Services
            // 5. Đăng ký Application Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<ISeatService, SeatService>();
            services.AddScoped<ICinemaRoomService, CinemaRoomService>();
            services.AddScoped<ISeatTypePriceService, SeatTypePriceService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IShowtimeService, ShowtimeService>();
            services.AddScoped<ISnackService, SnackService>();
            services.AddScoped<ISnackComboService, SnackComboService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ISeatScheduleService, SeatScheduleService>();
            services.AddScoped<IBackgroundService, BackgroundService>();    
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddScoped<IBackgroundService, BackgroundService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
           
            services.AddScoped<IVnPayService,VnPayService>();
            services.AddScoped<IScoreItemService, ScoreItemService>();

            #endregion
            //6.Đăng ký AutoMapper(scan toàn bộ assembly của Infrastructure để tìm Profile)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            //Đăng ký HangFire
            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(options =>
                {               
                    options.UseNpgsqlConnection(configuration.GetConnectionString("Local"));
                });
            });
            services.AddHangfireServer();
           
            //Đăng ký SignalR
            services.AddSignalR();
            //Đăng ký Elasticsearch
            services.AddSingleton(sp =>
            {
                var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                    .DefaultIndex("movies"); // Tùy bạn

                return new ElasticsearchClient(settings);
            });
            return services;
            
        }
    }
}
