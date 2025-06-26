using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class SeatScheduleForOrderResponse
    {
        public Guid Id { get; set; }
        public Guid? SeatId { get; set; }
        public string Label { get; set; } = string.Empty; // Nhãn ghế hiển thị (VD: A1, A2)
        public Guid? ShowtimeId { get; set; }

        public SeatBookingStatus Status { get; set; } = SeatBookingStatus.Available;
    }
}
