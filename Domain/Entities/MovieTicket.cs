using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class MovieTicket
{
    public Guid MovieTicketId { get; set; }

    public Guid? ShowtimeId { get; set; }

    public virtual Showtime? Showtime { get; set; }

    public virtual ICollection<TicketSeat> TicketSeats { get; set; } = new List<TicketSeat>();
}
