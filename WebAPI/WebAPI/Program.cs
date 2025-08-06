using Application.IServices;
using Domain.Entities;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Infrastructure;
using Infrastructure.Configuration;
using Infrastructure.Helper;
using Infrastructure.Identity;
using Infrastructure.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using WebAPI.Hubs;

namespace WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //FirebaseApp.Create(new AppOptions()
            //{
            //    Credential = GoogleCredential.FromFile("D:\\downloads\\OJT_BE\\team03_be\\WebAPI\\WebAPI\\student-51e6a-firebase-adminsdk-ix2ag-c9ca6c389c.json")
            //});
            //if (FirebaseApp.DefaultInstance == null)
            //{
            //    Console.WriteLine("FirebaseApp khởi tạo thất bại!");
            //}
            //else
            //{
            //    Console.WriteLine("FirebaseApp khởi tạo thành công!");
            //}

            // DI Infrastructure
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // Controllers & Swagger
            builder.Services
                .AddControllers()
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            // JWT config
            var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
            if (string.IsNullOrWhiteSpace(jwt.SecretKey))
                throw new InvalidOperationException("JwtSettings:SecretKey missing");

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwt.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddCors(o =>
            {
                o.AddPolicy("AllowAll",
                    p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
           

            var app = builder.Build();

            // Apply migration + Seed data (Role + SeatTypePrice) with comprehensive error handling
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    await SeedData.EnsureSeedDataAsync(services);
                    logger.LogInformation("Database seeding completed successfully.");
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    logger.LogError(ex, "PostgreSQL connection failed during seeding: {ErrorMessage}", ex.Message);
                    Console.WriteLine($"DATABASE ERROR: Cannot connect to PostgreSQL - {ex.Message}");
                    Console.WriteLine("Application will start without database seeding.");
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    logger.LogError(ex, "Database update failed during seeding: {ErrorMessage}", ex.Message);
                    Console.WriteLine($"DATABASE UPDATE ERROR: {ex.Message}");
                    Console.WriteLine("Application will start but database may be in inconsistent state.");
                }
                catch (TimeoutException ex)
                {
                    logger.LogError(ex, "Database connection timeout during seeding: {ErrorMessage}", ex.Message);
                    Console.WriteLine($"TIMEOUT ERROR: Database connection timed out - {ex.Message}");
                    Console.WriteLine("Application will start without database seeding.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error during database seeding: {ErrorMessage}", ex.Message);
                    Console.WriteLine($"GENERAL ERROR: Database seeding failed - {ex.Message}");
                    Console.WriteLine("Application will continue running without initial data.");
                }
            }
            //SignalR
            app.MapHub<SeatHub>("/seatHub");
            //HangFire
            app.UseHangfireDashboard();
            //Đăng ký tác vụ ngầm
            //mỗi phút,mỗi giờ,mỗi ngày trong tháng, tháng, ngày trong tuần ứng với mỗi *
            //RecurringJob.AddOrUpdate<IBackgroundService>("Change-Seat-Booking-status", s => s.ChangeSeatBookingStatus(), "* * * * *");
            RecurringJob.AddOrUpdate<IBackgroundService>("Is-Subscription-Expired", s => s.IsSubscriptionExpired(),Cron.Daily(0, 0)); 
            // Pipeline
            if (app.Environment.IsDevelopment())
            {
            }
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("AllowAll");

            // Add custom middleware to log all requests
           


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
