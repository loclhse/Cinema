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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await SeedRolesAsync(roleManager);
            await SeedSeatTypePricesAsync(context);
            await SeedUsersAsync(userManager, roleManager);
            await SeedGenresAndMoviesAsync(context);
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
                        SeatType = type,
                        DefaultPrice = 0    // hoặc giá mặc định tuỳ bạn
                    });
                }
            }

            await context.SaveChangesAsync();
        }
              /* ---------- Seed Users + AppUser Info ---------- */
        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<AppRole> roleManager)
        {
            async Task SeedUser(string username, string email, string password, string role, AppUser appUserInfo)
            {
                var existing = await userManager.FindByNameAsync(username);
                if (existing != null) return;

                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true,
                    AppUser = appUserInfo
                };

                await userManager.CreateAsync(user, password);
                if (await roleManager.RoleExistsAsync(role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            await SeedUser(
                "john_doe",
                "john.doe@example.com",
                "securePass123",
                "Member",
                new AppUser
                {
                    FullName = "John Doe",
                    Dob = DateOnly.Parse("1995-03-15"),
                    Phone = "+1234567890",
                    Address = "123 Main St, Cityville",
                    Sex = Sex.Male,
                    IdentityCard = "554825328199"
                });

            await SeedUser(
                "jane_smith",
                "jane.smith@company.com",
                "empPass456",
                "Employee",
                new AppUser
                {
                    FullName = "Jane Smith",
                    Dob = DateOnly.Parse("1988-07-22"),
                    Phone = "+1987654321",
                    Address = "456 Oak Ave, Townsville",
                    Sex = Sex.Female,
                    IdentityCard = "987654321123"
                });

            await SeedUser(
                "admin_bob",
                "bob.wilson@company.com",
                "adminSecure789",
                "Admin",
                new AppUser
                {
                    FullName = "Bob Wilson",
                    Dob = DateOnly.Parse("1980-11-30"),
                    Phone = "+1122334455",
                    Address = "789 Pine Rd, Metro City",
                    Sex = Sex.Male,
                    IdentityCard = "123456789012"
                });
        }

        /* ---------- Seed Genres, Movies, MovieGenres ---------- */
        private static async Task SeedGenresAndMoviesAsync(AppDbContext context)
        {
            if (!context.Genres.Any())
            {
                var animeGenre = new Genre
                {
                    Id = Guid.NewGuid(),
                    Name = "Anime",
                    Description = "Anime is a style of Japanese animation that has gained significant popularity outside of Japan..."
                };

                var genres = new List<Genre>
                {
                    new() { Id = Guid.NewGuid(), Name = "Action", Description = "Action-packed movies with thrilling sequences." },
                    new() { Id = Guid.NewGuid(), Name = "Comedy", Description = "Light-hearted movies designed to amuse." },
                    new() { Id = Guid.NewGuid(), Name = "Drama", Description = "Serious narratives focusing on character development." },
                    new() { Id = Guid.NewGuid(), Name = "Horror", Description = "Movies designed to scare and thrill." },
                    animeGenre
                };

                await context.Genres.AddRangeAsync(genres);
                await context.SaveChangesAsync();

                var movie = new Movie
                {
                    Id = Guid.NewGuid(),
                    Title = "Colorful Stage! The Movie: A Miku Who Can't Sing",
                    Description = "Shibuya teen musician Ichika meets Hatsune Miku after hearing her song in a store...",
                    ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
                    Duration = 110,
                    MovieStatus= MovieStatus.NowShowing,
                    Director = "Hiroyuki Hata",
                    Img = "https://m.media-amazon.com/images/M/MV5BOGRkNjk4MzktNjEyMi00MmIyLTkxODItZGI4M2QzMmI5ZThkXkEyXkFqcGc@._V1_.jpg",
                    TrailerUrl = "https://www.youtube.com/watch?v=3bD1dfiRMu4&pp=0gcJCfwAo7VqN5tD",
                    MovieGenres = new List<MovieGenre>
                    {
                        new MovieGenre { GenreId = animeGenre.Id }
                    }
                };

                await context.Movies.AddAsync(movie);
                await context.SaveChangesAsync();
            }
        }
    }
}
