using Application.IRepos;
using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MovieService : IMovieService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MovieService> _logger;

        public MovieService(ILogger<MovieService> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ApiResp> CreateMovieAsync(MovieRequest movieRequest)
        {
            ApiResp resp = new ApiResp();
            try
            {
                if (movieRequest.GenreIds == null || !movieRequest.GenreIds.Any())
                {
                    return resp.SetBadRequest("GenreIds cannot be null or empty.");
                }

                var movie = _mapper.Map<Movie>(movieRequest);
                if (movie == null)
                {
                    return resp.SetBadRequest("Invalid movie data.");
                }

                foreach (var genreId in movieRequest.GenreIds)
                {
                    // Ensure genreId is not null before dereferencing
                    if (genreId == Guid.Empty)
                    {
                        return resp.SetBadRequest("Invalid GenreId provided.");
                    }

                    var genre = await _unitOfWork.GenreRepo.GetAsync(x => x.Id == genreId && !x.IsDeleted);
                    if (genre == null)
                    {
                        return resp.SetBadRequest($"Genre with ID {genreId} not found.");
                    }
                    movie.MovieGenres.Add(new MovieGenre
                    {
                        Genre = genre,
                        Movie = movie
                    });
                }
                await _unitOfWork.MovieRepo.AddAsync(movie);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.elasticMovieRepo.IndexMovieAsync(movie);
                return resp.SetOk("Movie created successfully.");
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> DeleteMovieAsync(Guid id)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var movie = await _unitOfWork.MovieRepo.GetAsync(x => x.Id == id);
                if (movie == null)
                {
                    return resp.SetNotFound("Movie not found.");
                }
                movie.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return resp.SetOk("Movie deleted successfully.");
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetAllMoviesAsync()
        {
            //Tao mot list resp
            //trong response kh co truong Genres nen phai lay tu movies
            //sau do map tung cai
            //tra ve list response da xu li
            ApiResp resp = new ApiResp();
            try
            {
                var movies = await _unitOfWork.MovieRepo.GetAllAsync(x => !x.IsDeleted);
                var movieRespList = new List<MovieResponse>();
                if (movies == null || !movies.Any())
                {
                    return resp.SetNotFound("No movies found.");
                }
                foreach (var movie in movies)
                {
                    var GenreNames = await _unitOfWork.MovieRepo.GetGenreNamesForMovieAsync(movie.Id);
                    var response = _mapper.Map<MovieResponse>(movie);
                    response.ReleaseDate = movie.ReleaseDate;
                    response.EndDate = movie.EndDate;
                    response.GenreNames = GenreNames;
                    movieRespList.Add(response);

                }

                return resp.SetOk(movieRespList);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetMovieByIdAsync(Guid id)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var movie = await _unitOfWork.MovieRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                var Genres = await _unitOfWork.MovieRepo.GetGenreNamesForMovieAsync(movie.Id);

                var res = _mapper.Map<MovieResponse>(movie);
                res.ReleaseDate = movie.ReleaseDate;
                res.EndDate = movie.EndDate;
                res.GenreNames = Genres;
                if (movie == null)
                {
                    return resp.SetNotFound("Movie not found.");
                }
                return resp.SetOk(res);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UpdateMovieAsync(Guid id, MovieRequest movieRequest)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var movie = await _unitOfWork.MovieRepo.GetAsync(x => x.Id == id && !x.IsDeleted, include: query => query.Include(m => m.MovieGenres));

                if (movie == null)
                {
                    return resp.SetNotFound("Movie not found.");
                }

                if (movieRequest.GenreIds == null || !movieRequest.GenreIds.Any())
                {
                    return resp.SetBadRequest("GenreIds cannot be null or empty.");
                }
                var oldGenres = movie.MovieGenres.ToList();
                foreach (var mg in oldGenres)
                {
                    await _unitOfWork.MovieGenreRepo.RemoveByIdAsync(mg.Id);
                }

                foreach (var genreId in movieRequest.GenreIds)
                {
                    if (genreId == Guid.Empty)
                    {
                        return resp.SetBadRequest("Invalid GenreId provided.");
                    }

                    var genre = await _unitOfWork.GenreRepo.GetAsync(x => x.Id == genreId && !x.IsDeleted);
                    if (genre == null)
                    {
                        return resp.SetBadRequest($"Genre with ID {genreId} not found.");
                    }
                    await _unitOfWork.MovieGenreRepo.AddAsync(new MovieGenre
                    {
                        Genre = genre,
                        Movie = movie,
                    });
                }
                _mapper.Map(movieRequest, movie);
                await _unitOfWork.SaveChangesAsync();
                return resp.SetOk("Movie updated successfully.");
            }

            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }


        public async Task<ApiResp> SearchMoviesAsync(string searchTerm, string searchType, int? limit = 5)
        {


            try
            {
                var validSearchTypes = new[] { "all", "title", "director", "rated", "genres" };
                if (string.IsNullOrEmpty(searchType) || !validSearchTypes.Contains(searchType.ToLowerInvariant()))
                {
                    return new ApiResp()
                        .SetBadRequest(message: "Invalid or missing searchType. Valid options are: title, director, rated, moviegenres.");
                }

                IEnumerable<Movie> allMovies = await _unitOfWork.MovieRepo.SearchMoviesAsync(null);
                IEnumerable<Movie> movies;
                if (string.IsNullOrEmpty(searchTerm))
                {
                    movies = allMovies.Take(limit ?? 5);
                }
                else
                {
                    var searchLower = searchTerm.ToLowerInvariant();
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                    movies = allMovies.Where(m =>
                        (searchType.ToLowerInvariant() == "all" || searchType.ToLowerInvariant() == "title") &&
                        (m.Title != null && m.Title.ToLowerInvariant().Contains(searchLower)) ||
                        (searchType?.ToLowerInvariant() == "director" && m.Director != null && m.Director.ToLowerInvariant().Contains(searchLower)) ||
                        (searchType?.ToLowerInvariant() == "rated" && m.Rated != null && m.Rated.ToString().ToLowerInvariant().Contains(searchLower)) ||
                        (searchType?.ToLowerInvariant() == "genres" && m.MovieGenres != null && m.MovieGenres.Any(mg => mg.Genre != null && mg.Genre.Name != null && mg.Genre.Name.ToLowerInvariant().Contains(searchLower)))
                    ).OrderBy(m => m.Title).Take(limit ?? 5);
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                }


                if (!movies.Any())
                {
                    return new ApiResp()
                        .SetNotFound("No movies found");
                }
                var responses = _mapper.Map<List<MovieResponse>>(movies);
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

        //    public async Task<ApiResp> GetGenresForMovie(Guid movieId)
        //    {
        //    ApiResp resp = new ApiResp();
        //    try
        //    {
        //        var genreNames = await _unitOfWork.MovieRepo.GetGenreNamesForMovieAsync(movieId);
        //        return resp.SetOk(genreNames);
        //    }
        //    catch (Exception ex)
        //    {
        //        return resp.SetBadRequest(ex.Message);
        //    }
        //}
        public async Task<ApiResp> ElasticSearchMovie(string keyWord)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var movies = await _unitOfWork.elasticMovieRepo.elasticSearchMoviesAsync(keyWord);
                if (!movies.Any())
                {
                    return apiResp.SetNotFound("Not found");
                }
                var ids = movies.Select(m => m.Id).ToList();
                var moviesDBList = await _unitOfWork.MovieRepo.GetAllAsync(x => ids.Contains(x.Id) && !x.IsDeleted);
                var result = new List<MovieResponse>();
                foreach (var movie in moviesDBList)
                {
                    var GenreNames = await _unitOfWork.MovieRepo.GetGenreNamesForMovieAsync(movie.Id);
                    var response = _mapper.Map<MovieResponse>(movie);
                    response.GenreNames = GenreNames;
                    result.Add(response);
                }
                return apiResp.SetOk(result);

            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }
        public async Task<ApiResp> FindGenresNameByMovieNameAsync(string movieName)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var movies = await _unitOfWork.MovieRepo.GetAllAsync(x => x.Title.Contains(movieName) && !x.IsDeleted);
                if (movies == null || !movies.Any())
                {
                    return resp.SetNotFound("No movies found with the specified name.");
                }

                var genreNames = await _unitOfWork.MovieRepo.GetGenreNamesForMovieAsync(movies.First().Id);

                return resp.SetOk(genreNames);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetShowtimeByMovieId(Guid Id)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var movie = await _unitOfWork.MovieRepo.GetAsync(x => x.Id == Id && !x.IsDeleted, include: q => q.Include(m => m.Showtimes));
                if (movie == null)
                {
                    return resp.SetNotFound(message:"Movie not found!");
                }
                if (movie.Showtimes == null || !movie.Showtimes.Any())
                {
                    return resp.SetNotFound(message:"No showtimes found for this movie.");
                }
                var showtimeResponses = _mapper.Map<List<ShowtimeResponse>>(movie.Showtimes);
                return resp.SetOk(showtimeResponses);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }
    }
}
