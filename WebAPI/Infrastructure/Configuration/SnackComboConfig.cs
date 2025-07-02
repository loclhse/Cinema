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
    public class SnackComboConfig : IEntityTypeConfiguration<SnackComboItem>

    {
        public void Configure(EntityTypeBuilder<SnackComboItem> builder) {

            builder.HasOne(sci => sci.Combo)
                 .WithMany(sc => sc.SnackComboItems)
                 .HasForeignKey(sci => sci.ComboId)
                 .OnDelete(DeleteBehavior.Cascade)
                 .IsRequired(false); 

            builder.HasOne(sci => sci.Snack)
                   .WithMany(s => s.SnackComboItems)
                   .HasForeignKey(sci => sci.SnackId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired(false);

            builder.HasIndex(sci => new { sci.ComboId, sci.SnackId }).IsUnique();

           
            builder.HasQueryFilter(sci => !sci.IsDeleted);
        }
    }

    }

