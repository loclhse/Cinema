using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.IRepos
{
    public interface ISeatRepo : IGenericRepo<Seat>
    {
        Task<List<Seat>> GetByRoomAsync(Guid roomId);
        Task<List<Seat>> GetByScheduleAsync(Guid scheduleId);
        Task BulkUpdateSeatTypeAsync(IEnumerable<Guid> seatIds, SeatTypes newType);
        Task BulkUpdateSeatPriceAsync(object dto);
    }
}
