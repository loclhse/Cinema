using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class MovieTicket : BaseEntity
{

    public Guid? ShowtimeId { get; set; }
    public virtual Showtime? Showtime { get; set; }

    public virtual ICollection<TicketSeat> TicketSeats { get; set; } = new List<TicketSeat>();
}
