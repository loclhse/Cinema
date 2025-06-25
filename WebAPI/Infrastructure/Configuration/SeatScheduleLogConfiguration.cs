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
    public class SeatScheduleLogConfiguration : IEntityTypeConfiguration<SeatScheduleLog>
    {
        public void Configure(EntityTypeBuilder<SeatScheduleLog> builder)
        {
            builder.HasKey(t => t.Id);
            // quan he voi seat
            builder.HasOne(t => t.Seat)
                   .WithMany(s => s.SeatScheduleLogs)
                   .HasForeignKey(t => t.SeatId);

            // quan he voi showtime
            builder.HasOne(t => t.Showtime)
                   .WithMany(s => s.SeatScheduleLogs)
                   .HasForeignKey(t => t.ShowtimeId);

            // quan he voi order
            builder.HasOne(t => t.Order)
                   .WithMany(o => o.SeatScheduleLogs)
                   .HasForeignKey(t => t.OrderId);

        }
    }
}
