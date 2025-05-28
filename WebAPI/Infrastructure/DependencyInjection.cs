using Application;
using Application.IRepos;
using Application.IServices;
using Application.Services;
using Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static async Task<IServiceCollection> AddInfrastructureServicesAsync(this IServiceCollection services, IConfiguration config)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(config.GetConnectionString("Local"), npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            });

            #region repo config
            services.AddScoped<IUserRepo, UserRepo>();
            #endregion

            #region service config
            services.AddScoped<IUserService, UserService>();
            #endregion

            return services;
        }
    }
}
