using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CinemaRoom> CinemaRooms { get; set; }

    public virtual DbSet<CustomerScore> CustomerScores { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<MovieTicket> MovieTickets { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<ScoreHistory> ScoreHistories { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Showtime> Showtimes { get; set; }

    public virtual DbSet<Snack> Snacks { get; set; }

    public virtual DbSet<SnackCombo> SnackCombos { get; set; }

    public virtual DbSet<SnackComboItem> SnackComboItems { get; set; }

    public virtual DbSet<Theater> Theaters { get; set; }

    public virtual DbSet<TicketCancellationLog> TicketCancellationLogs { get; set; }

    public virtual DbSet<TicketSeat> TicketSeats { get; set; }

    public virtual DbSet<User> Users { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
