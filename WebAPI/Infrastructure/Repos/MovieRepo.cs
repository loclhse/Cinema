using Application.IRepos;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repos
{
    public class MovieRepo : GenericRepo<Movie>, IMovieRepo
    {
        public MovieRepo(AppDbContext context) : base(context)
        {
    
        }
        public async Task<List<string>> GetGenreNamesForMovieAsync(Guid movieId)
        {
            var movie = await GetAsync(
            m => m.Id == movieId && !m.IsDeleted,
            include: query => query.Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            );

            if (movie == null)
            {
                throw new Exception("Movie not found.");
            }

            var genreNames = movie.MovieGenres
                .Select(mg => mg.Genre.Name) // Lấy tên thể loại từ MovieGenre
                .ToList();

            return genreNames;
        }
    }
}
