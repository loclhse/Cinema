using Application.IServices;
using Application.ViewModel;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class RedeemService : IRedeemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public RedeemService(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResp> CreateRedeemAsync(Guid userId,List<RedeemRequest> requests)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var redeem = new Redeem();
                redeem.UserId = userId;
                
                    foreach (var item in requests)
                    {
                        var scoreItem = await _unitOfWork.ScoreItemRepo.GetByIdAsync(item.ScoreItemId);
                        if (scoreItem == null)
                        {
                            return apiResp.SetNotFound(message: "Score item not found");
                        }
                        if (item.Quantity > scoreItem.Quantity)
                        {
                            return apiResp.SetBadRequest(message: "The items are not enought for you to exchange!!!");
                        }
                        redeem.ScoreOrders.Add(new ScoreOrder
                        {
                            Quantity = item.Quantity,
                            ScoreItem = scoreItem,
                            Redeem = redeem
                        });
                        redeem.TotalScore += scoreItem.Score * item.Quantity;
                    }
                
                await _unitOfWork.redeemRepo.AddAsync(redeem);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Redeem create successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> GetRedeemAsync(Guid redeemId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var redeem = await _unitOfWork.redeemRepo.GetAsync(x => x.Id == redeemId && !x.IsDeleted);
                var ItemNames = await _unitOfWork.redeemRepo.GetItemNamesByRedeemId(redeemId);
                if (redeem == null)
                {
                    return apiResp.SetNotFound(message: "Redeem not found");
                }
                var redeemResponse = _mapper.Map<RedeemResponse>(redeem);
                redeemResponse.ItemNames = ItemNames;
                return apiResp.SetOk(redeemResponse);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> GetPendingRedeemsByAccountAsync(Guid accountId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var redeems = await _unitOfWork.redeemRepo.GetAllAsync(x => x.UserId == accountId && !x.IsDeleted && x.status == ScoreStatus.pending);
                var rs = new List<RedeemResponse>();
                if (redeems == null || !redeems.Any())
                {
                    return apiResp.SetNotFound(message: "No redeems found for this account");
                }
                foreach (var redeem in redeems)
                {
                    var itemName = await _unitOfWork.redeemRepo.GetItemNamesByRedeemId(redeem.Id);
                    var redeemResponse = _mapper.Map<RedeemResponse>(redeem);
                    redeemResponse.ItemNames = itemName;
                    rs.Add(redeemResponse);
                }
                return apiResp.SetOk(rs);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }
        public async Task<ApiResp> GetPaidRedeemsByAccountAsync(Guid accountId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var redeems = await _unitOfWork.redeemRepo.GetAllAsync(x => x.UserId == accountId && !x.IsDeleted && x.status == ScoreStatus.paid);
                var rs = new List<RedeemResponse>();
                if (redeems == null || !redeems.Any())
                {
                    return apiResp.SetNotFound(message: "No redeems found for this account");
                }
                foreach (var redeem in redeems)
                {
                    var itemName = await _unitOfWork.redeemRepo.GetItemNamesByRedeemId(redeem.Id);
                    var redeemResponse = _mapper.Map<RedeemResponse>(redeem);
                    redeemResponse.ItemNames = itemName;
                    rs.Add(redeemResponse);
                }
                return apiResp.SetOk(rs);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> GetAllRedeemsAsync()
        {
            var apiResp = new ApiResp();
            try
            {
                var redeems = await _unitOfWork.redeemRepo.GetAllAsync(x => !x.IsDeleted);
                var rs = new List<RedeemResponse>();
                if (redeems == null || !redeems.Any())
                {
                    return apiResp.SetNotFound(message: "No redeems found");
                }
                foreach (var redeem in redeems)
                {
                    var itemName = await _unitOfWork.redeemRepo.GetItemNamesByRedeemId(redeem.Id);
                    var redeemResponse = _mapper.Map<RedeemResponse>(redeem);
                    redeemResponse.ItemNames = itemName;
                    rs.Add(redeemResponse);
                }
                return apiResp.SetOk(rs);
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }

        public async Task<ApiResp> CancelRedeemAsync(Guid id)
        {
            var apiResp = new ApiResp();
            try
            {
                var redeem = await _unitOfWork.redeemRepo.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (redeem == null)
                {
                    return apiResp.SetNotFound(message: "Redeem not found");
                }
                redeem.status = ScoreStatus.cancelled;
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Redeem cancelled successfully!");

            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }
        public async Task<ApiResp> updateRedeemAsync(Guid redeemId, List<RedeemRequest> requests)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var redeem = await _unitOfWork.redeemRepo.GetAsync(x => x.Id == redeemId && !x.IsDeleted);
                if (redeem == null)
                {
                    return apiResp.SetNotFound(message: "Redeem not found");
                }
                redeem.ScoreOrders.Clear();
                redeem.TotalScore = 0;
                    foreach (var item in requests)
                    {
                        var scoreItem = await _unitOfWork.ScoreItemRepo.GetByIdAsync(item.ScoreItemId);
                        if (scoreItem == null)
                        {
                            return apiResp.SetNotFound(message: "Score item not found");
                        }
                        if (item.Quantity > scoreItem.Quantity)
                        {
                            return apiResp.SetBadRequest(message: "The items are not enough for you to exchange!!!");
                        }
                        await _unitOfWork.ScoreOrderRepo.AddAsync(new ScoreOrder
                        {
                            ScoreItem = scoreItem,
                            Redeem = redeem
                        });
                        redeem.TotalScore += scoreItem.Score * item.Quantity;
                    }
                
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Redeem updated successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }
        public async Task<ApiResp> redeemItem(Guid id)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
               
                var order = await _unitOfWork.redeemRepo.GetAsync(x => x.Id == id && !x.IsDeleted && x.status == ScoreStatus.pending);
                if (order == null)
                {
                    return apiResp.SetNotFound(null, "Redeem not found or already processed");
                }
                var userScore = await _unitOfWork.UserRepo.GetAsync(u => u.Id == order.UserId && !u.IsDeleted);
                if (userScore == null)
                {
                    return apiResp.SetNotFound(message: "Redeem not found or already processed");
                }
                if (userScore.Score < order.TotalScore)
                {
                    return apiResp.SetBadRequest(message: "Your score is not enough!");
                }
                order.status = ScoreStatus.paid;
               foreach(var item in order.ScoreOrders)
                {
                    var scoreItem = await _unitOfWork.ScoreItemRepo.GetByIdAsync(item.ScoreItemId);
                    if (scoreItem == null)
                    {
                        return apiResp.SetNotFound(message: "Score item not found");
                    }
                    scoreItem.Quantity -= item.Quantity;
                    scoreItem.Sold += item.Quantity;
                    if (scoreItem.Quantity < 0)
                    {
                        return apiResp.SetBadRequest(message: "The items are not enough for you to exchange!!!");
                    }
                    await _unitOfWork.ScoreItemRepo.UpdateAsync(scoreItem);
                }
                userScore.Score -= order.TotalScore;
                var ScoreLog = new ScoreLog
                {
                    UserId = order.UserId,
                    PointsChanged = $"-{order.TotalScore}",
                    ActionType = "Redeemed items from shop",
                };
                await _unitOfWork.ScoreLogRepo.AddAsync(ScoreLog);
                await _unitOfWork.UserRepo.UpdateAsync(userScore);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Redeem successful!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(null, ex.Message);
            }
        }
    }
}
