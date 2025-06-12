using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Configuration
{
    public class MovieConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.HasKey(m => m.Id); 
            builder.ToTable("Movies");

            builder.HasMany(m => m.Showtimes)
                   .WithOne(s => s.Movie)
                   .HasForeignKey(s => s.MovieId)
                   .IsRequired(false);

           
                  
        }
    }
}
