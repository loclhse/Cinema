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

        public SeatBookingStatus Status { get; set; } = SeatBookingStatus.Available;

        public DateTime? HoldUntil { get; set; } // Nullable nếu chưa hold

        public string? HoldByConnectionId { get; set; } // Dùng để phân biệt client khác

        public bool IsOwnedByCaller { get; set; } // Chỉ chính chủ giữ ghế này mới là true

        public int? HoldRemainingSeconds
        {
            get
            {
                if (HoldUntil.HasValue)
                {
                    var remaining = (int)(HoldUntil.Value - DateTime.UtcNow).TotalSeconds;
                    return remaining > 0 ? remaining : 0;
                }
                return null;
            }
        }
    }
}
