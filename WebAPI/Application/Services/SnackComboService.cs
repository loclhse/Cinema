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
               

                var combo = _mapper.Map<SnackCombo>(request);
                foreach (var snackItem in request.SnackItems) // Process each SnackItem with quantity
                {
                    var snack = await _uow.SnackRepo.GetByIdAsync(snackItem.SnackId);
                    if (snack == null)
                    {
                        return new ApiResp().SetNotFound(message: $"Snack with ID {snackItem.SnackId} not found.");
                    }
                    combo.SnackComboItems.Add(new SnackComboItem { SnackId = snackItem.SnackId, Quantity = snackItem.Quantity });
                }

                // Calculate total price based on snack prices, quantities, and discount
                combo.TotalPrice = CalculateTotalPrice(combo);

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

        private decimal CalculateTotalPrice(SnackCombo snackCombo)
        {
            if (snackCombo?.SnackComboItems == null || !snackCombo.SnackComboItems.Any())
                return 0;

            // Calculate subtotal from all snack items
            decimal subtotal = snackCombo.SnackComboItems
                .Where(item => item.Snack != null && !item.IsDeleted)
                .Sum(item => (item.Snack.Price * (item.Quantity ?? 1)));

            // Apply discount if available
            if (snackCombo.discount.HasValue && snackCombo.discount > 0)
            {
                decimal discountAmount = subtotal * (snackCombo.discount.Value / 100m);
                return subtotal - discountAmount;
            }

            return subtotal;
        }

        public async Task<ApiResp> AddSnackToComboAsync(Guid comboId, AddSnackToComboRequest request)
        {
            using var transaction = await _uow.BeginTransactionAsync();
            try
            {
                
                if (comboId == Guid.Empty)
                {
                    return new ApiResp().SetBadRequest("Invalid combo ID format.");
                }
                if (request == null || request.SnackId == Guid.Empty)
                {
                    return new ApiResp().SetBadRequest("Valid snack ID is required.");
                }
                

                
                var combo = await _uow.SnackComboRepo.GetComboWithItemsAsync(comboId);
                if (combo == null)
                {
                    return new ApiResp().SetNotFound("Snack combo not found.");
                }

               
                var snack = await _uow.SnackRepo.GetByIdAsync(request.SnackId);
                if (snack == null)
                {
                    return new ApiResp().SetNotFound($"Snack with ID {request.SnackId} not found.");
                }

                
                var existingItem = combo.SnackComboItems.FirstOrDefault(sci => sci.SnackId == request.SnackId && !sci.IsDeleted);
                if (existingItem != null)
                {
                    existingItem.Quantity += request.Quantity;
                    existingItem.UpdateDate = DateTime.UtcNow;
                }
                else
                {
                    var newComboItem = new SnackComboItem
                    {
                        ComboId = comboId,
                        SnackId = request.SnackId,
                        Quantity = request.Quantity
                    };
                   await _uow.SnackComboRepo.AddComboItemAsync(newComboItem);
                }

                // Recalculate total price after adding snack
                combo.TotalPrice = CalculateTotalPrice(combo);

                await _uow.SaveChangesAsync(); 
                await transaction.CommitAsync(); 

                // Get the updated combo for response
                var updatedCombo = await _uow.SnackComboRepo.GetComboWithItemsAsync(comboId);
                var response = _mapper.Map<SnackComboResponse>(updatedCombo);
                return new ApiResp().SetOk(response);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                return new ApiResp().SetBadRequest(message: $"Concurrency conflict: The combo may have been modified. Details: {ex.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResp().SetBadRequest(message: $"Error adding snack to combo: {ex.Message}");
            }
        }

        public async Task<ApiResp> GetAllSnackCombosAsync()
        {
            var resp = new ApiResp();
            try
            {
                var snackCombos = await _uow.SnackComboRepo.GetAllAsync(x => !x.IsDeleted, include: query => query.Include(sc => sc.SnackComboItems).ThenInclude(sc => sc.Snack));
                if (snackCombos == null || !snackCombos.Any())
                {
                    return resp.SetNotFound("No snack combos found.");
                }

                var responses = _mapper.Map<List<SnackComboResponse>>(snackCombos);
                return resp.SetOk(responses);
            }
            catch (Exception ex)
            {
                return resp.SetBadRequest(ex.Message);
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
                
                // Recalculate total price after removing snack
                combo.TotalPrice = CalculateTotalPrice(combo);
                
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
                    
                    // Recalculate total price after updating quantity
                    combo.TotalPrice = CalculateTotalPrice(combo);
                    
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
