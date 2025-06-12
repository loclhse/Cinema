using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IGenreService
    {
        Task<ApiResp> GetAllGenresAsync();
        Task<ApiResp> GetGenresAsync(Guid id);
        Task<ApiResp> CreateGenreAsync(GenreRequest genreRequest);
        Task<ApiResp> UpdateGenreAsync(Guid id, GenreRequest genreRequest);
        Task<ApiResp> DeleteGenreAsync(Guid id);
    }
}
