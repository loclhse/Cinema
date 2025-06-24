namespace Domain.Entities;

public partial class TicketLog : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? UserId { get; set; }
    public Guid SeatId { get; set; }
    public Guid ShowtimeId { get; set; }
    public virtual Seat? Seat { get; set; }
    public virtual Showtime? Showtime { get; set; }
    public virtual AppUser? User { get; set; }
    public virtual Order? Order { get; set; }
}
