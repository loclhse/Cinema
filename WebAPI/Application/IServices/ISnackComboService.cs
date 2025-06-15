using Application.ViewModel;
using Application.ViewModel.Request;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface ISnackComboService
    {
        Task<ApiResp> GetByIdAsync(Guid id);
        Task<ApiResp> GetAllAsync();
        Task<ApiResp> AddAsync(SnackComboRequest request);
        Task<ApiResp> UpdateAsync(Guid id, SnackComboRequest request);
        Task<ApiResp> DeleteAsync(Guid id);
        Task<ApiResp> GetCombosWithSnacksAsync();
        Task<ApiResp> GetComboWithItemsAsync(Guid id);
    }
}
