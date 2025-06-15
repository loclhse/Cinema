using Domain.Enums;
using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Seat : BaseEntity
{
    // Vị trí trong sơ đồ
    public int RowIndex { get; set; }  // Hàng (1-based)
    public int ColIndex { get; set; }  // Cột (1-based)

    // Nhãn ghế hiển thị (VD: A1, A2)
    public string? Label { get; set; }

    // Loại ghế: thường, VIP, đôi trái/phải, ghế trống
    public SeatTypes SeatType { get; set; }

    // Dùng cho ghế đôi (2 ghế có cùng GroupId)
    public Guid? CoupleGroupId { get; set; }

    // Trạng thái cấu hình
    public bool IsAvailable { get; set; } = true;  // Có thể bán không
    public bool IsActive { get; set; } = true;     // Có tồn tại trên sơ đồ không (nếu false thì là chỗ trống/lối đi)

    // Liên kết
    public Guid? CinemaRoomId { get; set; }
    public virtual CinemaRoom? CinemaRoom { get; set; }

    public virtual ICollection<SeatSchedule> SeatSchedules { get; set; } = new List<SeatSchedule>();

}
