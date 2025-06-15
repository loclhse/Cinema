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
    public class ShowtimeConfiguration : IEntityTypeConfiguration<Showtime>
    {
        public void Configure(EntityTypeBuilder<Showtime> builder)
        {
            builder.HasOne(s => s.CinemaRoom)
                  .WithMany(s => s.Showtimes)
                  .HasForeignKey(st => st.CinemaRoomId);
        }
    }
}
