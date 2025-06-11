using Application;
using Application.IRepos;
using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Service
{
    public class MovieService : IMovieService
    {

        private readonly IMovieRepo _movieRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MovieService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IMapper _mapper; 

        public MovieService(IMovieRepo movieRepository, IUnitOfWork unitOfWork,
        ILogger<MovieService> logger, IWebHostEnvironment environment, IMapper mapper)
        {
            _movieRepository = movieRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _environment = environment;
            _mapper = mapper; 
        }
        public async Task<ApiResp> SearchMoviesAsync(string searchTerm, string searchType, int? limit = 5)
        {
           

            try
            {
                var validSearchTypes = new[] {"all", "title", "director", "rated", "moviegenres" };
                if (string.IsNullOrEmpty(searchType) || !validSearchTypes.Contains(searchType.ToLowerInvariant()))
                {
                    return new ApiResp()
                        .SetBadRequest(message: "Invalid or missing searchType. Valid options are: title, director, rated, moviegenres.");
                }
                
                IEnumerable<Movie> allMovies = await _movieRepository.SearchMoviesAsync(null);
                IEnumerable<Movie> movies;
                if (string.IsNullOrEmpty(searchTerm))
                {
                    movies = allMovies.Take(limit ?? 5);
                }
                else
                {
                    var searchLower = searchTerm.ToLowerInvariant();
                    movies = allMovies.Where(m =>
                        (searchType.ToLowerInvariant() == "all" || searchType.ToLowerInvariant() == "title") &&
                        (m.Title != null && m.Title.ToLowerInvariant().Contains(searchLower)) ||
                        (searchType?.ToLowerInvariant() == "director" && m.Director != null && m.Director.ToLowerInvariant().Contains(searchLower)) ||
                        (searchType?.ToLowerInvariant() == "rated" && m.Rated != null && m.Rated.ToString().ToLowerInvariant().Contains(searchLower)) ||
                        (searchType?.ToLowerInvariant() == "moviegenres" && m.MovieGenres != null && m.MovieGenres.Any(mg => mg.Genre != null && mg.Genre.Name != null && mg.Genre.Name.ToLowerInvariant().Contains(searchLower)))
                    ).OrderBy(m => m.Title).Take(limit ?? 5);
                }

               
                if (!movies.Any())
                {
                    return new ApiResp()
                        .SetOk("No movies found");
                }

               
                var responses = _mapper.Map<List<MovieResponse>>(movies);
               

                foreach (var response in responses)
                {
                    if (response.Genres != null && response.Director != null && response.Rated != null)
                    {
                        var recommended = allMovies.Where(m =>
                            m.Id != response.Id &&
                            (m.MovieGenres != null && m.MovieGenres.Any(mg => response.Genres.Any(g => g != null && mg.Genre?.Name == g))) ||
                            (m.Director != null && m.Director.Equals(response.Director, StringComparison.OrdinalIgnoreCase)) ||
                            (m.Rated == response.Rated)
                        ).OrderBy(m => Guid.NewGuid()).Take(3).ToList();

                        response.RecommendedMovies = _mapper.Map<List<MovieResponse>>(recommended);
                    }
                }

                return new ApiResp()
                    .SetOk(result: responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching movies with term: {SearchTerm}, type: {SearchType}, limit: {Limit}", searchTerm ?? "null", searchType ?? "null", limit ?? 5);
                return new ApiResp()
                    .SetBadRequest(message: $"Failed to search movies. Please try again. Details: {ex.Message}");
            }
        }
    }
}
