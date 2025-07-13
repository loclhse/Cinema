using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class SnackOrderConfig
    {
        public void Configure(EntityTypeBuilder<SnackOrder> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Order)
                .WithMany(x => x.SnackOrders)
                .HasForeignKey(x => x.OrderId);
        }
    }
}
