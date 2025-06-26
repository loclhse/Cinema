using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IMovieService
    {
        Task<ApiResp> CreateMovieAsync(MovieRequest movieRequest);
        Task<ApiResp> GetAllMoviesAsync();
        Task<ApiResp> GetMovieByIdAsync(Guid id);
        Task<ApiResp> UpdateMovieAsync(Guid id, MovieRequest movieRequest);
        Task<ApiResp> DeleteMovieAsync(Guid id);
        Task<ApiResp> SearchMoviesAsync(string searchTerm, string searchType, int? limit = 5);
    }
}
