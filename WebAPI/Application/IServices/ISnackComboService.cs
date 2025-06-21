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
        
        Task<ApiResp> AddAsync(SnackComboRequest request);
        Task<ApiResp> UpdateAsync(Guid id, SnackComboUpdateRequest request);
        Task<ApiResp> DeleteAsync(Guid id);
       
        Task<ApiResp> GetComboWithItemsAsync(Guid id);
        Task<ApiResp> UpdateSnackQuantityInComboAsync(Guid comboId, Guid snackId, int quantity);

        Task<ApiResp> AddSnackToComboAsync(Guid comboId, AddSnackToComboRequest request);
        Task<ApiResp> DeleteSnackFromComboAsync(Guid comboId, Guid snackId);

        Task<ApiResp> GetAllSnackCombosAsync();
    }
}
