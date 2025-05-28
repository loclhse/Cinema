using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class MovieTicket
{
    public int MovieTicketId { get; set; }

    public int? ShowtimeId { get; set; }

    public virtual Showtime? Showtime { get; set; }

    public virtual ICollection<TicketSeat> TicketSeats { get; set; } = new List<TicketSeat>();
}
