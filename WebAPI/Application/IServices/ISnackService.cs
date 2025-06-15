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
    public interface ISnackService
    {
        Task<ApiResp> GetByIdAsync(Guid id);
        Task<ApiResp> GetAllAsync();
        Task<ApiResp> AddAsync(SnackRequest request);
        Task<ApiResp> DeleteAsync(Guid id);

        Task<ApiResp> UpdateAsync(Guid id, SnackRequest request);
        Task<ApiResp> GetSnacksInComboAsync(Guid comboId);
    }
}


