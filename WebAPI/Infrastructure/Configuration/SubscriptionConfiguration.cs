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
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.HasKey(s => s.Id);
            //quan he voi subscription plan
            builder.HasOne(s => s.SubscriptionPlan)
                   .WithMany(o => o.Subscriptions)
                   .HasForeignKey(s => s.SubscriptionPlanId)
                   .OnDelete(DeleteBehavior.Cascade);
            //quan he voi payment
            builder.HasMany(s => s.Payments)
                   .WithOne(p => p.Subscription)
                   .HasForeignKey(p => p.SubscriptionId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
