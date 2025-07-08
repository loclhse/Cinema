using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ScoreItemService : IScoreItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ScoreItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<ApiResp> CreateNewItemAsync(ItemRequest request)
        {
            var apiResp = new ApiResp();
            try
            {
                var item = _mapper.Map<ScoreItem>(request);
                if(item.Quantity == 0)
                {
                    return apiResp.SetBadRequest( message:"Quantity must be greater than 0");
                }
                await _unitOfWork.ScoreItemRepo.AddAsync(item);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk($"Item create successfully: {item.Id}");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> DeleteItemAsync(Guid id)
        {
           var aResp = new ApiResp();
            try
            {
                var item = await _unitOfWork.ScoreItemRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (item == null)
                {
                    return aResp.SetNotFound(message: "Item not found");
                }
                item.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();
                return aResp.SetOk($"Item with id {id} deleted successfully");
            }
            catch (Exception ex)
            {
               return aResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetAllItemsAsync(int page, int size)
        {
            var aResp = new ApiResp();
                try
                {
                   var items = await _unitOfWork.ScoreItemRepo.GetAllAsync(x => !x.IsDeleted, null, page, size);
                    if (items == null || !items.Any())
                    {
                        return aResp.SetNotFound(message: "No items found");
                    }
                    var itemResponses = _mapper.Map<List<ItemResponse>>(items);
                    return aResp.SetOk(itemResponses);
            }
                catch (Exception ex)
                {
                    return aResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetItemByIdAsync(Guid id)
        {
            var aResp = new ApiResp();
            try
            {
                var item = await _unitOfWork.ScoreItemRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (item == null)
                {
                    return aResp.SetNotFound(message: "Item not found");
                }
                var itemResponse = _mapper.Map<ItemResponse>(item);
                return aResp.SetOk(itemResponse);
            }
            catch (Exception ex)
            {
                return aResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> UpdateItemAsync(Guid id, ItemRequest request)
        {
            var aResp = new ApiResp();
            try
            { 
                var item = await _unitOfWork.ScoreItemRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (item == null)
                {
                    return aResp.SetNotFound(message: "Item not found");
                }
                if (request.Quantity == 0)
                {
                    return aResp.SetBadRequest(message: "Quantity must be greater than 0");
                }
                _mapper.Map(request, item); 
                await _unitOfWork.SaveChangesAsync();
                return aResp.SetOk($"Updated Successfully");
            }
            catch (Exception ex)
            {
                return aResp.SetBadRequest(ex.Message);
            }
        }
    }
}
