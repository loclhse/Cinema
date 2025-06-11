using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class MovieGenreConfiguration : IEntityTypeConfiguration<MovieGenre>
    {
        public void Configure(EntityTypeBuilder<MovieGenre> builder)
        {
            // Nếu dùng Id riêng cho entity:
            builder.HasKey(mg => mg.Id);
            // Nếu dùng khóa kép (MovieId + GenreId):
            //builder.HasKey(mg => new { mg.MovieId, mg.GenreId });

            builder.ToTable("MovieGenres");

            builder.HasOne(mg => mg.Movie)
                   .WithMany(m => m.MovieGenres)
                   .HasForeignKey(mg => mg.MovieId);

            builder.HasOne(mg => mg.Genre)
                   .WithMany(g => g.MovieGenres)
                   .HasForeignKey(mg => mg.GenreId);

            builder.HasKey(m => m.Id); // Explicitly set Id as primary key
            
           
        }
    }
}
