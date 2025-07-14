using Domain.Entities;
using Infrastructure.Identity; // chứa ApplicationUser
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Infrastructure.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // 1. Primary key is Id
            builder.HasKey(u => u.Id);

            // 2. Use Id as a foreign key → ApplicationUser.Id
            // AppUser has one ApplicationUser
            builder
                .HasOne<ApplicationUser>() // Fix: Ensure the method is properly chained to the builder
                .WithOne(u => u.AppUser)   // Specify ApplicationUser.AppUser as the inverse navigation property
                .HasForeignKey<AppUser>(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Prevent EF from generating Id – it will use Id from ApplicationUser
            builder
                .Property(u => u.Id)
                .ValueGeneratedNever();

            // 4. Additional configurations
            builder
                .Property(u => u.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(u => u.Sex)
                .HasConversion<string>();
            //quan he voi subscription
            builder.HasMany(u => u.Subscriptions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            //quan he voi redeem
            builder.HasMany(u => u.Redeems)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.ScoreLog)
                .WithOne(x => x.AppUser)
                .HasForeignKey(x => x.UserId);
        }
    }
}
