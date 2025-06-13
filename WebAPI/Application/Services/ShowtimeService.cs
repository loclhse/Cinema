using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ShowtimeService : IShowtimeService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public ShowtimeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        private async Task<bool> IsShowtimeExistsAsync(Guid id)
        {
            var request = await _unitOfWork.ShowtimeRepo.GetAsync(s => s.Id == id && !s.IsDeleted);
            var Exists = await _unitOfWork.ShowtimeRepo.GetAllAsync(s => !s.IsDeleted);
            foreach (var item in Exists)
            {
                if (item.Date == request.Date && item.CinemaRoomId == request.CinemaRoomId && item.MovieId == request.MovieId)
                {
                    if ((item.StartTime < request.EndTime && request.StartTime < item.EndTime) ||
                        (request.StartTime < item.EndTime && item.StartTime < request.EndTime))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public async Task<ApiResp> CreateShowtimeAsync(ShowtimeResquest showtimeResquest)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var showtime = _mapper.Map<Showtime>(showtimeResquest);
                if (showtime == null)
                {
                    return apiResp.SetBadRequest("Invalid showtime data.");
                }
                var movie = await _unitOfWork.MovieRepo.GetAsync(m => m.Id == showtimeResquest.MovieId && !m.IsDeleted);
                if (movie == null)
                {
                    return apiResp.SetNotFound("Movie does not exist!");
                }
                showtime.Duration = movie.Duration + 45;
                showtime.EndTime = showtime.StartTime.AddMinutes((double)showtime.Duration);
                if (await IsShowtimeExistsAsync(showtime.Id))
                {
                    return apiResp.SetBadRequest("Showtime already exists in the same cinema room and date.");
                }
                await _unitOfWork.ShowtimeRepo.AddAsync(showtime);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Added successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> DeleteShowtimeAsync(Guid id)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var showtime = await _unitOfWork.ShowtimeRepo.GetAsync(x => x.Id == id);
                if (showtime == null)
                {
                    return apiResp.SetNotFound("Showtime not found.");
                }
                showtime.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Deleted successfully!");
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(ex.Message);
            }
        }
        public async Task<ApiResp> GetAllShowtimesAsync()
        {
            var resp = new ApiResp();
            try
            {
                var showtimes = await _unitOfWork.ShowtimeRepo.GetAllAsync(s => !s.IsDeleted);
                if (showtimes == null || !showtimes.Any())
                {
                    return resp.SetNotFound("No showtimes found.");
                }
                var result = _mapper.Map<List<ShowtimeResponse>>(showtimes);
                return resp.SetOk(result);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetShowtimeByIdAsync(Guid id)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var showtime = await _unitOfWork.ShowtimeRepo.GetAsync(s => s.Id == id && !s.IsDeleted);
                if (showtime == null)
                {
                    return resp.SetNotFound("Showtime not found.");
                }
                var result = _mapper.Map<ShowtimeResponse>(showtime);
                return resp.SetOk(result);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UpdateShowtimeAsync(Guid id, ShowtimeResquest showtimeResquest)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var showtime = await _unitOfWork.ShowtimeRepo.GetAsync(s => s.Id == id && !s.IsDeleted);
                if (showtime == null)
                {
                    return resp.SetNotFound("Showtime not found.");
                }
                var movie = await _unitOfWork.MovieRepo.GetAsync(m => m.Id == showtimeResquest.MovieId && !m.IsDeleted);
                _mapper.Map(showtimeResquest, showtime);
                showtime.Duration = movie.Duration + 45;
                showtime.EndTime = showtime.StartTime.AddMinutes((double)showtime.Duration);
                if (await IsShowtimeExistsAsync(showtime.Id))
                {
                    return resp.SetBadRequest("Showtime already exists in the same cinema room and date.");
                }
                await _unitOfWork.SaveChangesAsync();
                return resp.SetOk("Updated successfully!");
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
            }

        }
    }
}
