using Application.ViewModel;
using Application.ViewModel.Response;
using System;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IDashboardService
    {
        Task<ApiResp> GetDashboardDataAsync();
        Task<ApiResp> GetMovieRankingsAsync(int? limit = null);
    }
} 