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
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(o => o.Id);
            //quan he voi Appuser
            builder.HasOne(o => o.User)
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.UserId);
       
            //quan he voi payment
            builder.HasMany(o => o.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId);
             
            //quan he voi seat schedule
            builder.HasMany(o => o.SeatSchedules)
                .WithOne(s => s.Order)
                .HasForeignKey(s => s.OrderId);
     
            //quan he voi ticket log
            builder.HasMany(o => o.SeatScheduleLogs)
                .WithOne(t => t.Order)
                .HasForeignKey(t => t.OrderId);
        }
    }
}
