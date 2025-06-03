using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Showtime:BaseEntity
{
   

    public Guid? MovieId { get; set; }

    public Guid? RoomId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Movie? Movie { get; set; }

    public virtual ICollection<MovieTicket> MovieTickets { get; set; } = new List<MovieTicket>();

    public virtual CinemaRoom? Room { get; set; }
}
