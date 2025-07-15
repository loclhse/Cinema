using Application.IRepos;
using Application.ViewModel.Request;
using Domain.Entities;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
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
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            var movie = await GetAsync(
            m => m.Id == movieId && !m.IsDeleted,
            include: query => query.Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            );
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            if (movie == null)
            {
                throw new Exception("Movie not found.");
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var genreNames = movie.MovieGenres
                .Select(mg => mg.Genre.Name) 
                .ToList();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            return genreNames;
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }


#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public async Task<IEnumerable<Movie>> SearchMoviesAsync(string? searchTerm, string searchType, int? limit = 5)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            IQueryable<Movie> query = _db
                .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre);


            if (string.IsNullOrEmpty(searchTerm))
            {

                return await query.Take(limit ?? int.MaxValue).ToListAsync();
            }

            var searchLower = searchTerm.ToLowerInvariant();
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            query = query.Where(m =>
                (searchType.ToLowerInvariant() == "all" || searchType.ToLowerInvariant() == "title") &&
                (m.Title != null && m.Title.ToLowerInvariant().Contains(searchLower)) ||
                (searchType.ToLowerInvariant() == "director" && m.Director != null && m.Director.ToLowerInvariant().Contains(searchLower)) ||
                (searchType.ToLowerInvariant() == "rated" && m.Rated != null && m.Rated.ToString().ToLowerInvariant().Contains(searchLower)) ||
                (searchType.ToLowerInvariant() == "genres" && m.MovieGenres != null && m.MovieGenres.Any(mg => mg.Genre != null && mg.Genre.Name != null && mg.Genre.Name.ToLowerInvariant().Contains(searchLower)))
            );
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

            return await query
                .OrderBy(m => m.Title)
                .Take(limit ?? int.MaxValue)
                .ToListAsync();
        }
       
    }

}


