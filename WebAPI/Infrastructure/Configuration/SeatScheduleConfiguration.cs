using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class SeatScheduleConfiguration : IEntityTypeConfiguration<SeatSchedule>
    {
        public void Configure(EntityTypeBuilder<SeatSchedule> builder)
        {
            builder.HasKey(ss => ss.Id);

            builder.ToTable("SeatSchedules");

            builder.HasOne(s => s.Seat)
                   .WithMany(ss => ss.SeatSchedules)
                   .HasForeignKey(s => s.SeatId);

            builder.HasOne(s => s.Showtime)
                   .WithMany(s => s.SeatSchedules)
                   .HasForeignKey(mg => mg.ShowtimeId);
        }
    }
}
