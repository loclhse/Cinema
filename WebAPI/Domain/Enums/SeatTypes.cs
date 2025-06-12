using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum SeatTypes
    {
        Regular = 0,      // Ghế thường
        VIP = 1,          // Ghế VIP
        CoupleLeft = 2,   // Ghế đôi bên trái
        CoupleRight = 3,  // Ghế đôi bên phải
        None = 4          // Không phải ghế (để biểu diễn khoảng trống/lối đi nếu cần)
    }
}
