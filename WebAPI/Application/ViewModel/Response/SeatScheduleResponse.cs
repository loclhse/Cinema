using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;

namespace Application.ViewModel.Response
{
    public class SeatScheduleResponse
    {
        public Guid Id { get; set; }
        public Guid? SeatId { get; set; }
        public Guid? ShowtimeId { get; set; }
        public DateTime? HoldUntil { get; set; } // Nullable, chỉ có giá trị nếu Status = Held
        public SeatBookingStatus Status { get; set; } = SeatBookingStatus.Available;
    }
}
