using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.ViewModel.Request
{
    public class UpdateSeatStatusRequest
    {
        public List<Guid> SeatScheduleIds { get; set; } = new();
        public SeatBookingStatus Status { get; set; }
    }
}
