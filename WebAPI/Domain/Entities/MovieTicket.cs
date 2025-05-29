using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class MovieTicket : BaseEntity
{

    public int? ShowtimeId { get; set; }
    public virtual Showtime? Showtime { get; set; }

    public virtual ICollection<TicketSeat> TicketSeats { get; set; } = new List<TicketSeat>();
}
