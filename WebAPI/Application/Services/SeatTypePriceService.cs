using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class SeatTypePriceService : ISeatTypePriceService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public SeatTypePriceService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        /// <summary>
        /// Lấy toàn bộ bảng giá (để FE render form chỉnh giá)
        /// </summary>
        public async Task<IReadOnlyList<SeatTypePriceResponse>> GetAllAsync()
        {
            var entities = await _uow.SeatTypePriceRepo.GetAllAsync();
            return _mapper.Map<List<SeatTypePriceResponse>>(entities);
        }

        /// <summary>
        /// Lấy giá của 1 SeatType (nếu cần API riêng lẻ)
        /// </summary>
        public async Task<SeatTypePriceResponse?> GetBySeatTypeAsync(SeatTypes type)
        {
            var entity = await _uow.SeatTypePriceRepo.GetAsync(x => x.SeatType == type);
            return entity == null ? null : _mapper.Map<SeatTypePriceResponse>(entity);
        }

        /// <summary>
        /// Cập nhật giá 1 loại ghế
        /// </summary>
        public async Task<SeatTypePriceResponse> UpdateAsync(SeatTypes seatType, SeatTypePriceUpdateRequest request)
        {
            var entity = await _uow.SeatTypePriceRepo.GetAsync(x => x.SeatType == seatType)
                         ?? throw new KeyNotFoundException($"SeatTypePrice for '{seatType}' not found.");

            entity.DefaultPrice = (decimal)request.NewPrice;
            await _uow.SaveChangesAsync();

            return _mapper.Map<SeatTypePriceResponse>(entity);
        }
    }
}
