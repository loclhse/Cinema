using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IServices;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class SeatService : ISeatService
    {
        public Task BulkUpdateSeatPriceAsync(object dto)
        {
            throw new NotImplementedException();
        }

        public Task BulkUpdateSeatTypeAsync(IEnumerable<Guid> seatIds, SeatTypes newType)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfirmSeatsAsync(object req)
        {
            throw new NotImplementedException();
        }

        public Task<List<Seat>> GetSeatsByRoomAsync(Guid roomId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Seat>> GetSeatsByScheduleAsync(Guid scheduleId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HoldSeatsAsync(object req)
        {
            throw new NotImplementedException();
        }

        public Task ReleaseExpiredHoldsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Seat> UpdateSeatAsync(Guid seatId, object dto)
        {
            throw new NotImplementedException();
        }
    }
}
