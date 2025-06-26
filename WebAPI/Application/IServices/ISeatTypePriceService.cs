using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Domain.Entities;
using Domain.Enums;

namespace Application.IServices
{
    public interface ISeatTypePriceService
    {
        /// Lấy toàn bộ bảng giá (để FE render form chỉnh giá)
        Task<IReadOnlyList<SeatTypePriceResponse>> GetAllAsync();

        /// Lấy giá của 1 SeatType (nếu cần API riêng lẻ)
        Task<SeatTypePriceResponse?> GetBySeatTypeAsync(SeatTypes type);

        /// Cập nhật giá 1 loại ghế
        Task<SeatTypePriceResponse> UpdateAsync(SeatTypes seatTypes, SeatTypePriceUpdateRequest request);
    }
}
