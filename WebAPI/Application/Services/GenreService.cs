using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class GenreService : IGenreService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public GenreService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<ApiResp> CreateGenreAsync(GenreRequest genreRequest)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var genre = _mapper.Map<Genre>(genreRequest);
                if (genre == null)
                {
                    return resp.SetBadRequest(null, "Enter Data please!!!");
                }
                var checkGere = await _uow.GenreRepo.GetAsync(x => x.Name == genre.Name && !x.IsDeleted);
                if (checkGere != null)
                {
                    return resp.SetBadRequest(null, "Genre already exists!!!");
                }
                await _uow.GenreRepo.AddAsync(genre);
                await _uow.SaveChangesAsync();
                return resp.SetOk(genre);

            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> DeleteGenreAsync(Guid id)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var genre = await _uow.GenreRepo.GetAsync(x => x.Id == id);
                if (genre == null)
                {
                    return resp.SetNotFound(null, "Genre not found!!!");
                }
                genre.IsDeleted = true;
                await _uow.SaveChangesAsync();
                return resp.SetOk("Deleted successfully");

            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> GetAllGenresAsync()
        {
            ApiResp resp = new ApiResp();
            try
            {
                var genres = await _uow.GenreRepo.GetAllAsync(x => x.IsDeleted != true);
                var genreDtos = _mapper.Map<List<GenreResponse>>(genres);
                if (!genreDtos.Any())
                {
                    return resp.SetNotFound(null, "No genres found.");
                }
                return resp.SetOk(genreDtos);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> GetGenresAsync(Guid id)
        {
            var resp = new ApiResp();
            try
            {
                var genre = await _uow.GenreRepo.GetAsync(x => x.Id == id && x.IsDeleted != true);
                if (genre == null)
                {
                    return resp.SetNotFound(null, "Genre does not exist!!");
                }
                var genreDto = _mapper.Map<GenreResponse>(genre);
                return resp.SetOk(genreDto);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> UpdateGenreAsync(Guid id, GenreRequest genreRequest)
        {
            var resp = new ApiResp();
            try
            {
                var genre = await _uow.GenreRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (genre == null)
                {
                    return resp.SetNotFound(null, "Genre does not exist!!");
                }
                var checkGere = await _uow.GenreRepo.GetAsync(x => x.Name == genreRequest.Name && x.IsDeleted != true && genre.Name != x.Name);
                if (checkGere != null)
                {
                    return resp.SetBadRequest(null, "Genre already exists!!!");
                }
                _mapper.Map(genreRequest, genre);
                await _uow.SaveChangesAsync();
                return resp.SetOk("Update successfully!!!");

            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(null, ex.Message);
            }
        }

    }
}
