using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Identity;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Helper
{
    public static class SeedData
    {
        /// <summary>
        /// Hàm duy nhất bạn gọi trong Program.cs
        /// </summary>
        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            var context     = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedRolesAsync(roleManager);
            await SeedSeatTypePricesAsync(context);
        }

        /* ---------- Seed Roles ---------- */
        private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
        {
            var roles = new[] { "Admin", "Employee", "Member", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new AppRole(role));
                }
            }
        }

        /* ---------- Seed SeatTypePrice ---------- */
        private static async Task SeedSeatTypePricesAsync(AppDbContext context)
        {
            // Lấy danh sách SeatTypePrice hiện có (hash set cho nhanh)
            var existingTypes = await context.SeatTypePrices
                                             .Select(x => x.SeatType)
                                             .ToListAsync();

            foreach (SeatTypes type in Enum.GetValues(typeof(SeatTypes)))
            {
                if (!existingTypes.Contains(type))
                {
                    context.SeatTypePrices.Add(new SeatTypePrice
                    {
                        SeatType     = type,
                        DefaultPrice = 0    // hoặc giá mặc định tuỳ bạn
                    });
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
