using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.IServices
{
    public interface a
    {
        // --- Ghế theo phòng ---
        Task<List<Seat>> GetSeatsByRoomAsync(Guid roomId);
        Task<Seat?> GetSeatByIdAsync(Guid seatId);

        // --- Cập nhật cấu hình ---
        Task UpdateSeatTypeAsync(IEnumerable<Guid> seatIds, SeatTypes newType);
        Task UpdateSeatAvailabilityAsync(IEnumerable<Guid> seatIds, bool isAvailable);
    }
}
