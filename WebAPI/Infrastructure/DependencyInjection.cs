using System;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Repos;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Application;
using Infrastructure.Service;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;

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
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 4. Đăng ký JwtTokenGenerator (sinh JWT)
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            #endregion

            #region Services
            // 5. Đăng ký Application Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            #endregion
            //6.Đăng ký AutoMapper(scan toàn bộ assembly của Infrastructure để tìm Profile)
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            return services;
        }
    }
}
