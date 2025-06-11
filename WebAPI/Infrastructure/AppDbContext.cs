using Domain.Entities;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class AppDbContext : IdentityDbContext<ApplicationUser, AppRole, Guid>
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CinemaRoom> CinemaRooms { get; set; }

    //public virtual DbSet<CustomerScore> CustomerScores { get; set; }
    public virtual DbSet<Genre> Genres { get; set; } 

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<MovieGenre> MovieGenres { get; set; }

    //public virtual DbSet<MovieTicket> MovieTickets { get; set; }

    //public virtual DbSet<Order> Orders { get; set; }

    //public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    //public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    //public virtual DbSet<ScoreHistory> ScoreHistories { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatSchedule> SeatSchedules { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    //public virtual DbSet<Snack> Snacks { get; set; }

    //public virtual DbSet<SnackCombo> SnackCombos { get; set; }

    //public virtual DbSet<SnackComboItem> SnackComboItems { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
    public DbSet<OtpValid> OtpValids { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Áp dụng tất cả các IEntityTypeConfiguration<T> trong cùng assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
