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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.ConstrainedExecution;
using System.Text.RegularExpressions;
using System.Threading.Channels;

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

                return new ApiResp().SetOk(response).SetApiResponse(HttpStatusCode.Created, true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Rollback on error
                return new ApiResp().SetBadRequest(message: $"Error adding snack combo: {ex.Message}");
            }
        }

        public async Task<ApiResp> DeleteSnackFromComboAsync(Guid comboId, Guid snackId)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                var combo = await _uow.SnackComboRepo.GetComboWithItemsAsync(comboId);
                if (combo == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack combo not found.");
                }

                var item = combo.SnackComboItems.FirstOrDefault(sci => sci.SnackId == snackId && !sci.IsDeleted);
                if (item == null)
                {
                    return new ApiResp().SetNotFound(message: $"Snack item with SnackId {snackId} not found in combo.");
                }

                item.IsDeleted = true;
                item.UpdateDate = DateTime.UtcNow;
                await _uow.SnackComboRepo.UpdateAsync(combo);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ApiResp().SetOk("Snack removed from combo successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResp().SetBadRequest(message: $"Error deleting snack from combo: {ex.Message}");
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


        public async Task<ApiResp> UpdateSnackQuantityInComboAsync(Guid comboId, Guid snackId, int quantity)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                var combo = await _uow.SnackComboRepo.GetComboWithItemsAsync(comboId);
                if (combo == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack combo not found.");
                }

                var item = combo.SnackComboItems.FirstOrDefault(sci => sci.SnackId == snackId && !sci.IsDeleted);
                if (item == null)
                {
                    return new ApiResp().SetNotFound(message: $"Snack item with SnackId {snackId} not found in combo.");
                }
            else
                {
                    item.Quantity = quantity;
                    item.UpdateDate = DateTime.UtcNow;
                    await _uow.SnackComboRepo.UpdateAsync(combo);
                }

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ApiResp().SetOk("Snack quantity updated successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResp().SetBadRequest(message: $"Error updating snack quantity: {ex.Message}");
            }
        }

        public async Task<ApiResp> AddSnackToComboAsync(Guid comboId, Guid snackId, int quantity)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                var combo = await _uow.SnackComboRepo.GetComboWithItemsAsync(comboId);
                if (combo == null)
                {
                    return new ApiResp().SetNotFound(message: "Snack combo not found.");
                }

                var snack = await _uow.SnackRepo.GetByIdAsync(snackId);
                if (snack == null)
                {
                    return new ApiResp().SetNotFound(message: $"Snack with ID {snackId} not found.");
                }

                if (quantity <= 0)
                {
                    return new ApiResp().SetBadRequest(message: "Quantity must be greater than zero.");
                }

                var existingItem = combo.SnackComboItems.FirstOrDefault(sci => sci.SnackId == snackId && !sci.IsDeleted);
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    _uow.SnackComboRepo.UpdateAsync(combo);
                }
                else
                {
                    var newItem = new SnackComboItem
                    {
                        ComboId = comboId,
                        SnackId = snackId,
                        Quantity = quantity,
                        UpdateDate = DateTime.UtcNow
                    };
                    combo.SnackComboItems.Add(newItem);
                    _uow.SnackComboRepo.UpdateAsync(combo);

                }

                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();
                return new ApiResp().SetOk("Snack added to combo successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResp().SetBadRequest(message: $"Error adding snack to combo: {ex.Message}");
            }
        }

      
            public async Task<ApiResp> UpdateAsync(Guid id, SnackComboUpdateRequest request)
            {
                var resp = new ApiResp();
                try
                {
                   
                    if (id == Guid.Empty)
                    {
                        return resp.SetBadRequest("Invalid ID format.");
                    }
                    if (request == null)
                    {
                        return resp.SetBadRequest("Request body cannot be null.");
                    }

                   
                    var existingCombo = await _uow.SnackComboRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                    if (existingCombo == null)
                    {
                        return resp.SetNotFound($"Snack combo with ID {id} not found.");
                    }

                   
                    if (string.IsNullOrWhiteSpace(request.Name))
                    {
                        return resp.SetBadRequest("Name is required.");
                    }
                    if (request.TotalPrice <= 0)
                    {
                        return resp.SetBadRequest("Total price must be greater than zero.");
                    }

                    
                    _mapper.Map(request, existingCombo);
                    existingCombo.UpdateDate = DateTime.UtcNow; // Update timestamp

  await _uow.SaveChangesAsync();
                    return resp.SetOk("Snack combo updated successfully.");
                }
                catch (Exception ex)
                {
                    return resp.SetBadRequest(ex.Message);
                }
            }

        }

    }
