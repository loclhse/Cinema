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
    public class ScoreConfiguration : IEntityTypeConfiguration<ScoreOrder>
    {
        public void Configure(EntityTypeBuilder<ScoreOrder> builder)
        {
           builder.HasOne(c => c.Redeem)
                .WithMany(c => c.ScoreOrders)
                .HasForeignKey(c => c.RedeemId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(c => c.ScoreItem)
                .WithMany()
                .HasForeignKey(c => c.ScoreItemId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure primary key
            builder.HasKey(c => c.Id);
        }
    }
}
