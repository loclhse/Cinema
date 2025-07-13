using Application.ViewModel;
using Application.ViewModel.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IScoreItemService
    {
        Task<ApiResp> CreateNewItemAsync(ItemRequest request);
        Task<ApiResp> UpdateItemAsync(Guid id, ItemRequest request);
        Task<ApiResp> DeleteItemAsync(Guid id);
        Task<ApiResp> GetItemByIdAsync(Guid id);
        Task<ApiResp> GetAllItemsAsync(int page, int size);
    }
}
