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
    public class RoomLayoutConfiguration : IEntityTypeConfiguration<RoomLayout>
    {
        public void Configure(EntityTypeBuilder<RoomLayout> builder)
        {
            builder.HasKey(r => r.Id);
            builder.ToTable("RoomLayout");

            // Npgsql sẽ tự serialize/deserialize JsonDocument <=> jsonb
            builder.Property(c => c.LayoutJson)
                   .HasColumnType("jsonb");

            builder.HasOne(r => r.CinemaRoom)
                   .WithMany(s => s.RoomLayouts)
                   .HasForeignKey(s => s.CinemaRoomId);
        }
    }
}
