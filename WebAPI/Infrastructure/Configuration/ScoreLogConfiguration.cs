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
    public class ScoreLogConfiguration : IEntityTypeConfiguration<ScoreLog>
    {
        public void Configure(EntityTypeBuilder<ScoreLog> builder)
        {
            builder.HasOne(x => x.AppUser)
                .WithMany(x => x.ScoreLog)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.UserId).IsRequired();
        }
    }
}
