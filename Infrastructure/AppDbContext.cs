using Infrastructure.Entities;
using Infrastructure.Enums;
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

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
       

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Identitycart).HasColumnName("identity_card");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.Address).HasColumnName("address").HasMaxLength(255);
            entity.Property(e => e.Sex).HasColumnName("sex").HasConversion(v => v.ToString(), v => (Sex)Enum.Parse(typeof(Sex), v)).IsRequired();
            entity.Property(e => e.role).HasColumnName("role").HasConversion(v => v.ToString(), v => (Role)Enum.Parse(typeof(Role), v)).IsRequired();
            entity.Property(e => e.CreateDate).HasColumnName("create_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdateDate).HasColumnName("update_date").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

           
            entity.HasOne(u => u.CustomerScore)
                  .WithOne(cs => cs.User)
                  .HasForeignKey<CustomerScore>(cs => cs.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            
            entity.HasMany(u => u.Orders)
                  .WithOne(o => o.User)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Promotions)
                  .WithOne(p => p.User)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.ScoreHistories)
                  .WithOne(sh => sh.User)
                  .HasForeignKey(sh => sh.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Theaters)
                  .WithOne(t => t.User)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.TicketCancellationLogs)
                  .WithOne(tcl => tcl.User)
                  .HasForeignKey(tcl => tcl.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

    }
}

