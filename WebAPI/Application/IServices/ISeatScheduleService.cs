using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.ViewModel.Response;
using Domain.Enums;

namespace Application.IServices
{
    public interface ISeatScheduleService
    {
        // Cập nhật trạng thái ghế (Available, Booked, Held)
        Task<OperationResult> UpdateSeatStatusAsync(List<Guid> seatScheduleIds, SeatBookingStatus status);

        Task<SeatScheduleResponse?> GetShowTimeBySeatScheduleAsync(Guid id);

        // Giữ ghế (Hold) với thời gian giữ
        Task<IEnumerable<SeatScheduleResponse>> HoldSeatAsync(Guid showtimeId, List<Guid> seatIds, Guid userId, string connectionId);

        // Lấy tất cả seat schedules theo ShowtimeId
        Task<IEnumerable<SeatScheduleResponse>> GetSeatSchedulesByShowtimeAsync(Guid showtimeId);
        Task<IEnumerable<SeatScheduleResponse>> GetHoldSeatByUserIdAsync(Guid showtimeId, Guid userId);
        // Xác nhận giữ ghế (Confirm)
        Task<OperationResult> ConfirmSeatAsync(List<Guid> seatIds, Guid userId);
        // Huỷ giữ ghế (Cancel Hold)
        Task<OperationResult> CancelHoldAsync(List<Guid> seatIds, Guid userId);
        // Huỷ giữ ghế theo ConnectionId (dùng trong SignalR)
        Task CancelHoldByConnectionAsync(string connectionId, Guid userId);
    }
}
