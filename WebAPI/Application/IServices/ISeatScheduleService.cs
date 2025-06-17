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

        // Giữ ghế (Hold) với thời gian giữ
        Task<IEnumerable<SeatScheduleResponse>> HoldSeatAsync(List<Guid> seatIds);

        // Lấy tất cả seat schedules theo ShowtimeId
        Task<IEnumerable<SeatScheduleResponse>> GetSeatSchedulesByShowtimeAsync(Guid showtimeId);

        // Lấy seat schedule theo SeatId
        Task<SeatScheduleResponse> GetSeatScheduleBySeatIdAsync(Guid seatId);
    }
}
