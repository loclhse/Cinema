using Application.IRepos;
using Application.IServices;
using Application.ViewModel.Response;
using Application.ViewModel;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Application.ViewModel.Request;

namespace Application.Services
{
    public class SnackComboService : ISnackComboService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SnackComboService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<ApiResp> GetByIdAsync(Guid id)
        {
            try
            {
                var combo = await _uow.SnackComboRepo.GetByIdAsync(id);
                if (combo == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack combo not found.");
                }
                var response = _mapper.Map<SnackComboResponse>(combo);
                return new ApiResp().SetOk(response);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving snack combo: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetAllAsync()
        {
            try
            {
                var combos = await _uow.SnackComboRepo.GetAllAsync(e => !e.IsDeleted);
                var responses = _mapper.Map<IEnumerable<SnackComboResponse>>(combos);
                return new ApiResp().SetOk(responses);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving snack combos: {ex.Message}");
            }
        }

        public async Task<ApiResp> AddAsync(SnackComboRequest request)
        {
            using var transaction = await _uow.BeginTransactionAsync(); // Start transaction
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return new ApiResp().SetBadRequest(message: "Snack combo name is required.");
                }
                if (request.TotalPrice <= 0)
                {
                    return new ApiResp().SetBadRequest(message: "Total price must be greater than zero.");
                }
                if (request.SnackIds == null || !request.SnackIds.Any())
                {
                    return new ApiResp().SetBadRequest(message: "At least one snack is required.");
                }

                var combo = _mapper.Map<SnackCombo>(request);
                foreach (var snackId in request.SnackIds) // Process each SnackId individually
                {
                    var snack = await _uow.SnackRepo.GetByIdAsync(snackId);
                    if (snack == null)
                    {
                        return new ApiResp().SetNotFound(message: $"Snack with ID {snackId} not found.");
                    }
                    combo.SnackComboItems.Add(new SnackComboItem { SnackId = snackId, Quantity = 1 });
                }

                await _uow.SnackComboRepo.AddAsync(combo);
                await _uow.SaveChangesAsync(); // Commit within transaction
                await transaction.CommitAsync();

                var response = _mapper.Map<SnackComboResponse>(combo);
                response.SnackIds = (IEnumerable<Guid>?)combo.SnackComboItems.Select(sci => sci.SnackId);
                return new ApiResp().SetOk(response).SetApiResponse(HttpStatusCode.Created, true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback on error
                return new ApiResp().SetBadRequest(message: $"Error adding snack combo: {ex.Message}");
            }
        }

        public async Task<ApiResp> UpdateAsync(Guid id, SnackComboRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Name))
                {
                    return new ApiResp().SetBadRequest(message: "Snack combo name is required.");
                }
                if (request.TotalPrice <= 0)
                {
                    return new ApiResp().SetBadRequest(message: "Total price must be greater than zero.");
                }

                var existingCombo = await _uow.SnackComboRepo.GetByIdAsync(id);
                if (existingCombo == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack combo not found.");
                }

                _mapper.Map(request, existingCombo);
                if (request.SnackIds != null)
                {
                    existingCombo.SnackComboItems.Clear();
                    foreach (var snackId in request.SnackIds)
                    {
                        var snack = await _uow.SnackRepo.GetByIdAsync(snackId);
                        if (snack == null)
                        {
                            return new ApiResp().SetNotFound(message: $"Snack with ID {snackId} not found.");
                        }
                        existingCombo.SnackComboItems.Add(new SnackComboItem { SnackId = snackId, Quantity = 1 });
                    }
                }

                await _uow.SnackComboRepo.UpdateAsync(existingCombo);
                await _uow.SaveChangesAsync();
                var response = _mapper.Map<SnackComboResponse>(existingCombo);
                return new ApiResp().SetOk(response);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error updating snack combo: {ex.Message}");
            }
        }



        public async Task<ApiResp> GetComboWithItemsAsync(Guid id)
        {
            try
            {
                var combo = await _uow.SnackComboRepo.GetComboWithItemsAsync(id);
                if (combo == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack combo not found.");
                }
                var response = _mapper.Map<SnackComboResponse>(combo);
               
                return new ApiResp().SetOk(response);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving combo with items: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetCombosWithSnacksAsync()
        {
            try
            {
                var combos = await _uow.SnackComboRepo.GetCombosWithSnacksAsync();
                var responses = _mapper.Map<IEnumerable<SnackComboResponse>>(combos);
                return new ApiResp().SetOk(responses);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error retrieving combos with snacks: {ex.Message}");
            }
        }

        public async Task<ApiResp> DeleteAsync(Guid id)
        {
            try
            {
                await _uow.SnackComboRepo.DeleteAsync(id);
                return new ApiResp().SetOk("Snack combo deleted successfully.");
            }
            catch (KeyNotFoundException ex)
            {
                return new ApiResp().SetNotFound(message: ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiResp().SetBadRequest(message: $"Error deleting snack combo: {ex.Message}");
            }
        }
    }
}
    
