using Application;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Infrastructure.MapperConfigs;
using Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Drawing;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public  static IServiceCollection AddInfrastructureServicesAsync(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(typeof(MapperConfig));
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("Local"), npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            });

            services.AddScoped<IUserRepo, UserRepo>();

            // Services
            services.AddScoped<IUserService, UserService>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
