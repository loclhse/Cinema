using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Configuration
{
    public class SeatTypeConfigConfiguration : IEntityTypeConfiguration<SeatTypePrice>
    {
        public void Configure(EntityTypeBuilder<SeatTypePrice> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ToTable("SeatTypePrice");

            // Enum -> string
            builder.Property(x => x.SeatType)
                   .HasConversion<string>();            // "VIP", "Regular", …

            builder.Property(x => x.DefaultPrice)
                   .IsRequired();

            // (Tuỳ chọn) nếu mỗi loại ghế chỉ có 1 giá duy nhất:
            builder.HasIndex(x => x.SeatType)
                   .IsUnique();

            // (Tuỳ chọn) nếu bạn cho phép cấu hình theo từng phòng:
            // builder.HasIndex(x => new { x.SeatType, x.CinemaRoomId })
            //        .IsUnique();
        }
    }
}
