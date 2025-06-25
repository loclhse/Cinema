using Domain.Enums;

namespace Domain.Entities;

public partial class SeatScheduleLog : BaseEntity
{
    public Guid SeatId { get; set; }
    public Guid ShowtimeId { get; set; }
    public Guid? OrderId { get; set; }

    public SeatBookingStatus Status { get; set; } = SeatBookingStatus.Available;

    public virtual Seat? Seat { get; set; }
    public virtual Showtime? Showtime { get; set; }
    public virtual Order? Order { get; set; }
}
