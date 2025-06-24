using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class TicketLogConfiguration : IEntityTypeConfiguration<TicketLog>
    {
        public void Configure(EntityTypeBuilder<TicketLog> builder)
        {
            builder.HasKey(t => t.Id);
            // quan he voi seat
            builder.HasOne(t => t.Seat)
                   .WithMany(s => s.TicketLogs)
                   .HasForeignKey(t => t.SeatId)
                   .OnDelete(DeleteBehavior.Cascade);
            // quan he voi showtime
            builder.HasOne(t => t.Showtime)
                   .WithMany(s => s.TicketLogs)
                   .HasForeignKey(t => t.ShowtimeId)
                   .OnDelete(DeleteBehavior.Cascade);
            // quan he voi user
            builder.HasOne(t => t.User)
                   .WithMany(u => u.TicketLogs)
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
            // quan he voi order
            builder.HasOne(t => t.Order)
                   .WithMany(o => o.TicketLogs)
                   .HasForeignKey(t => t.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
