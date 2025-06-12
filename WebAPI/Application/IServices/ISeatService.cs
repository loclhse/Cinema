using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.IServices
{
    public interface ISeatService
    {
        Task<List<Seat>> GetSeatsByRoomAsync(Guid roomId);
        Task<List<Seat>> GetSeatsByScheduleAsync(Guid scheduleId);

        Task<Seat> UpdateSeatAsync(Guid seatId, object dto);
        Task BulkUpdateSeatTypeAsync(IEnumerable<Guid> seatIds, SeatTypes newType);
        Task BulkUpdateSeatPriceAsync(object dto);

        // ====== Đặt vé ======
        Task<bool> HoldSeatsAsync(object req);          // Status = Held
        Task<bool> ConfirmSeatsAsync(object req);    // Status = Booked
        Task ReleaseExpiredHoldsAsync();                          // Cron job
    }
}
