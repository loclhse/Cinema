using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepos;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;

namespace Infrastructure.Repos
{
    public class SeatRepo : GenericRepo<Seat>, ISeatRepo
    {
        private new readonly AppDbContext _context;
        public SeatRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public Task BulkUpdateSeatPriceAsync(object dto)
        {
            throw new NotImplementedException();
        }

        public Task BulkUpdateSeatTypeAsync(IEnumerable<Guid> seatIds, SeatTypes newType)
        {
            throw new NotImplementedException();
        }

        public Task<List<Seat>> GetByRoomAsync(Guid roomId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Seat>> GetByScheduleAsync(Guid scheduleId)
        {
            throw new NotImplementedException();
        }
    }
}
