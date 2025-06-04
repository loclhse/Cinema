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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            // 1. Khóa chính
            builder.HasKey(e => e.Id);

            // 2. Mapping bảng (nếu muốn đổi tên)
            builder.ToTable("RefreshTokens");

            // 3. Cột Token (bắt buộc, có thể max length nếu bạn cần)
            builder
                .Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(200);

            // 4. Cột ExpiresAt (bắt buộc)
            builder
                .Property(e => e.ExpiresAt)
                .IsRequired();

            // 5. Quan hệ 1-N với AppUser
            builder
                .HasOne(e => e.AppUser)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
