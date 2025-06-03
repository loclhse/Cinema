using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class TicketSeat:BaseEntity
{
   

    public Guid? MovieTicketId { get; set; }

    public Guid? SeatId { get; set; }

    public virtual MovieTicket? MovieTicket { get; set; }

    public virtual Seat? Seat { get; set; }
}
