using Application.IRepos;
using Domain.Entities;
using Domain.Enums;
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
        private readonly AppDbContext _context;

        public MovieRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }




        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string? searchTerm, string searchType, int? limit = 5)
        {
            IQueryable<Movie> query = _db
                .Include(m => m.Showtimes)
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre);
                

            if (string.IsNullOrEmpty(searchTerm))
            {
              
                return await query.Take(limit ?? int.MaxValue).ToListAsync();
            }

            var searchLower = searchTerm.ToLowerInvariant();
            query = query.Where(m =>
                (searchType.ToLowerInvariant() == "all" || searchType.ToLowerInvariant() == "Title") &&
                (m.Title != null && m.Title.ToLowerInvariant().Contains(searchLower)) ||
                (searchType.ToLowerInvariant() == "director" && m.Director != null && m.Director.ToLowerInvariant().Contains(searchLower)) ||
                (searchType.ToLowerInvariant() == "rated" && m.Rated != null && m.Rated.ToString().ToLowerInvariant().Contains(searchLower)) ||
                (searchType.ToLowerInvariant() == "moviegenres" && m.MovieGenres != null && m.MovieGenres.Any(mg => mg.Genre != null && mg.Genre.Name != null && mg.Genre.Name.ToLowerInvariant().Contains(searchLower)))
            );

            return await query
                .OrderBy(m => m.Title) 
                .Take(limit ?? int.MaxValue) 
                .ToListAsync();
        }


    }
    }
    

