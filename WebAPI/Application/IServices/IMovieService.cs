using Application.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IMovieService
    {
        Task<ApiResp> SearchMoviesAsync(string searchTerm, string searchType, int? limit = 5);
    }
}
