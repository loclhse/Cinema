using Application.IRepos;
using Application.IServices;
using Application.ViewModel.Response;
using Application.ViewModel;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ViewModel.Request;
using System.Net;

namespace Application.Services
{
    public class SnackService : ISnackService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SnackService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

       
        public async Task<ApiResp> GetByIdAsync(Guid id)
        {
            try
            {
                var snack = await _uow.SnackRepo.GetByIdAsync(id);
                if (snack == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack not found.");
                }
                var response = _mapper.Map<SnackResponse>(snack);
                return new ApiResp().SetOk(response);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving snack: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetAllAsync()
        {
            try
            {
                var snacks = await _uow.SnackRepo.GetAllAsync(e => !e.IsDeleted);
                var responses = _mapper.Map<IEnumerable<SnackResponse>>(snacks);
                return new ApiResp().SetOk(responses);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving snacks: {ex.Message}");
            }
        }



        public async Task<ApiResp> AddAsync(SnackRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return new ApiResp().SetBadRequest(message: "Snack name is required.");
                }
                if (request.Quantity <= 0)
                {
                    return new ApiResp().SetBadRequest(message: "Quantity must be greater than zero.");
                }

                var snack = _mapper.Map<Snack>(request);
                await _uow.SnackRepo.AddAsync(snack);
                await _uow.SaveChangesAsync(); 
                var response = _mapper.Map<SnackResponse>(snack);
                return new ApiResp().SetOk(response).SetApiResponse(HttpStatusCode.Created, true);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error adding snack: {ex.Message}");
            }
        }

        public async Task<ApiResp> UpdateAsync(Guid id, SnackRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return new ApiResp().SetBadRequest(message: "Snack name is required.");
                }
                if (request.Quantity <= 0)
                {
                    return new ApiResp().SetBadRequest(message: "Quantity must be greater than zero.");
                }

                var existingSnack = await _uow.SnackRepo.GetByIdAsync(id);
                if (existingSnack == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack not found.");
                }

                _mapper.Map(request, existingSnack);
                await _uow.SnackRepo.UpdateAsync(existingSnack);
                var response = _mapper.Map<SnackResponse>(existingSnack);
                return new ApiResp().SetOk(response);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error updating snack: {ex.Message}");
            }
        }


        public async Task<ApiResp> DeleteAsync(Guid id)
                
        {
             try
                   {
                      await _uow.SnackRepo.DeleteAsync(id);
                      return new ApiResp().SetOk("Snack deleted successfully.");
                   }
             catch (KeyNotFoundException ex)
                    {
                      return new ApiResp().SetNotFound(message: ex.Message);
                    }
             catch (Exception ex)
                    {
                      return new ApiResp().SetBadRequest(message: $"Error deleting snack: {ex.Message}");
                    }
                }

        public async Task<ApiResp> GetSnacksInComboAsync(Guid comboId)
        {
            try
            {
                var snacks = await _uow.SnackRepo.GetSnacksInComboAsync(comboId);
                if (!snacks.Any())
                {
                    return new ApiResp().SetNotFound(message: "No snacks found for this combo.");
                }
                var responses = _mapper.Map<IEnumerable<SnackResponse>>(snacks);
                return new ApiResp().SetOk(responses);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving snacks in combo: {ex.Message}");
            }
        }

       
    }
}