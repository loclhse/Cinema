using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class TicketSeat
{
    public int TicketSeatId { get; set; }

    public int? MovieTicketId { get; set; }

    public int? SeatId { get; set; }

    public virtual MovieTicket? MovieTicket { get; set; }

    public virtual Seat? Seat { get; set; }
}
