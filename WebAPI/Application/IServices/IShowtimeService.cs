using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IShowtimeService
    {
        Task<ApiResp> GetAllShowtimesAsync();
        Task<ApiResp> GetShowtimeByIdAsync(Guid id);
        Task<ApiResp> CreateShowtimeAsync(ShowtimeResquest showtimeResquest, Guid movieId, Guid roomId);
        Task<ApiResp> UpdateShowtimeAsync(Guid id, ShowtimeUpdateRequest showtimeUpdateRequest);
        Task<ApiResp> DeleteShowtimeAsync(Guid id);
        Task<ApiResp> GetShowtimeByMovieIdAsync(Guid id);

    }
}
