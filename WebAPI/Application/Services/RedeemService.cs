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

        public async Task<ApiResp> CreateRedeemAsync(RedeemRequest request)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                if (request.ScoreItemId == null || !request.ScoreItemId.Any())
                {
                    return apiResp.SetBadRequest("Score item IDs cannot be empty");
                }
                var user = _httpContextAccessor.HttpContext.User;
                var userId = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return apiResp.SetUnauthorized(message: "User not authenticated");
                }
                var redeem = _mapper.Map<Redeem>(request);
                redeem.UserId = Guid.Parse(userId.Value); 
                foreach(var item in request.ScoreItemId)
                {
                    var scoreItem = await _unitOfWork.ScoreItemRepo.GetByIdAsync(item);
                    if (scoreItem == null)
                    {
                        return apiResp.SetNotFound("Score item not found");
                    }
                    if(redeem.Quantity > scoreItem.Quantity)
                    {
                        return apiResp.SetBadRequest("The items are not enought for you to exchange!!!");
                    }
                    scoreItem.Quantity -= redeem.Quantity;
                    redeem.ScoreOrders.Add(new ScoreOrder
                    {
                        ScoreItem = scoreItem,
                        Redeem = redeem
                    });
                    redeem.TotalScore += scoreItem.Score * redeem.Quantity;
                }
                await _unitOfWork.redeemRepo.AddAsync(redeem);
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Redeem create successfully!");
            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
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
                    return apiResp.SetNotFound("Redeem not found");
                }
                var redeemResponse = _mapper.Map<RedeemResponse>(redeem);
                redeemResponse.ItemNames = ItemNames;
                return apiResp.SetOk(redeemResponse);
            }
            catch(Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }

        public async Task<ApiResp> GetRedeemsByAccountAsync(Guid accountId)
        {
            ApiResp apiResp = new ApiResp();
            try
            {
                var redeems = await _unitOfWork.redeemRepo.GetAllAsync(x => x.UserId == accountId && !x.IsDeleted);
                var rs = new List<RedeemResponse>();
                if (redeems == null || !redeems.Any())
                {
                    return apiResp.SetNotFound("No redeems found for this account");
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
                return apiResp.SetBadRequest(ex.Message);
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
                    return apiResp.SetNotFound("No redeems found");
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
                return apiResp.SetBadRequest(ex.Message);
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
                    return apiResp.SetNotFound("Redeem not found");
                }
                redeem.status = ScoreStatus.cancelled;
                await _unitOfWork.SaveChangesAsync();
                return apiResp.SetOk("Redeem cancelled successfully!");

            }
            catch (Exception ex)
            {
                return apiResp.SetBadRequest(ex.Message);
            }
        }
    }
}
