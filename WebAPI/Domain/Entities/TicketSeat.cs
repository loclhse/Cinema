namespace Domain.Entities;

public partial class TicketSeat :BaseEntity
{
    public int? MovieTicketId { get; set; }
    public virtual MovieTicket? MovieTicket { get; set; }

    public int? SeatId { get; set; }
    public virtual Seat? Seat { get; set; }
}
