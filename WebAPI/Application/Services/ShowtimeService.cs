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
        private async Task<bool> IsShowtimeExistsAsync(DateTime date, Guid cinemaRoomId, DateTime startTime, DateTime endTime)
        {
            // Tìm kiếm buổi chiếu đã tồn tại trong cùng một ngày và phòng chiếu
            var existingShowtimes = await _unitOfWork.ShowtimeRepo.GetAllAsync(s =>
                s.CinemaRoomId == cinemaRoomId &&
                s.Date.Date == date.Date &&
                !s.IsDeleted // Nếu có trường IsDeleted
            );

            // Kiểm tra xem có buổi chiếu nào chồng chéo không
            foreach (var showtime in existingShowtimes)
            {
                // Kiểm tra chồng chéo
                if ((startTime < showtime.EndTime && endTime > showtime.StartTime)  ||
                    (startTime < showtime.EndTime && endTime > showtime.EndTime)    ||
                    (startTime < showtime.StartTime && endTime > showtime.StartTime)
                    )
                {
                    return true; // Có chồng chéo
                }
            }

            return false; // Không có chồng chéo
        }

        public async Task<ApiResp> CreateShowtimeAsync(ShowtimeResquest showtimeResquest, Guid movieId, Guid roomId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var showtime = _mapper.Map<Showtime>(showtimeResquest);
                if (showtime == null)
                {
                    return apiResp.SetBadRequest("Invalid showtime data.");
                }
                var movie = await _unitOfWork.MovieRepo.GetAsync(m => m.Id == movieId && !m.IsDeleted);
                if (movie == null)
                {
                    return apiResp.SetNotFound("Movie does not exist!");
                }
                var cinemaRoom = await _unitOfWork.CinemaRoomRepo.GetAsync(c => c.Id == roomId && !c.IsDeleted);
                if (cinemaRoom == null)
                {
                    return apiResp.SetNotFound("Cinema room does not exist!");
                }
                showtime.Duration = movie.Duration + 45;
                showtime.EndTime = showtime.StartTime.AddMinutes((double)showtime.Duration);
                showtime.MovieId = movie.Id;
                showtime.CinemaRoomId = cinemaRoom.Id;
                if (await IsShowtimeExistsAsync(showtime.Date, showtime.CinemaRoomId, showtime.StartTime, showtime.EndTime) == true)
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

        public async Task<ApiResp> UpdateShowtimeAsync(Guid id, ShowtimeUpdateRequest showtimeUpdateRequest)
        {
            ApiResp resp = new ApiResp();
            try
            {
                var showtime = await _unitOfWork.ShowtimeRepo.GetAsync(s => s.Id == id && !s.IsDeleted);
                if (showtime == null)
                {
                    return resp.SetNotFound("Showtime not found.");
                }
                var movie = await _unitOfWork.MovieRepo.GetAsync(m => m.Id == showtimeUpdateRequest.MovieId && !m.IsDeleted);
                if(movie == null)
                {
                    return resp.SetNotFound("Movie does not exist!!!");
                }
                var room = await _unitOfWork.CinemaRoomRepo.GetAsync(c => c.Id == showtimeUpdateRequest.CinemaRoomId && !c.IsDeleted);
                if(room == null)
                {
                    return resp.SetNotFound("Cinema room does not exist!!!");
                }
                _mapper.Map(showtimeUpdateRequest, showtime);
                showtime.Duration = movie.Duration + 45;
                showtime.EndTime = showtime.StartTime.AddMinutes((double)showtime.Duration);
                if (await IsShowtimeExistsAsync(showtime.Date, showtime.CinemaRoomId, showtime.StartTime, showtime.EndTime) == true)
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
