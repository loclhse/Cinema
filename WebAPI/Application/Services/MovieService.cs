using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
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
        public MovieService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResp> CreateMovieAsync(MovieRequest movieRequest)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var movie = _mapper.Map<Movie>(movieRequest);
                foreach(var genreId in movieRequest.GenreIds)
                {
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
                if (movie == null)
                {
                    return resp.SetBadRequest("Invalid movie data.");    
                }
                await _unitOfWork.MovieRepo.AddAsync(movie);
                await _unitOfWork.SaveChangesAsync();
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
                var movie = await _unitOfWork.MovieRepo.GetAsync(x=> x.Id == id);
                if (movie == null)
                {
                    return resp.SetNotFound("Movie not found.");
                }
                movie.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return resp.SetOk("Movie deleted successfully.");
            }
            catch(Exception ex)
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

        //Huy chỉnh phần này nha chỉnh cho nó hiện ra thể loại chứ ban đầu get không thì tên thế loại là rỗng
        public async Task<ApiResp> GetMovieByIdAsync(Guid id)
        {
            ApiResp resp = new ApiResp();
            try { 
                var movie = await _unitOfWork.MovieRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (movie == null)
                {
                    return resp.SetNotFound("Movie not found.");
                }
                return resp.SetOk(movie);
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
                movie.MovieGenres.Clear();
                foreach (var genreId in movieRequest.GenreIds)
                {
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
                _mapper.Map(movieRequest, movie);
                await _unitOfWork.SaveChangesAsync();
                return resp.SetOk("Movie updated successfully.");
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
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
    }
    
}
