using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum SeatBookingStatus
    {
        Available,     // Mặc định chưa ai đặt
        Hold,          // Đang giữ tạm (chưa thanh toán)
        Booked,        // Đã được đặt thành công
        Cancelled      // Bị huỷ
    }
}
