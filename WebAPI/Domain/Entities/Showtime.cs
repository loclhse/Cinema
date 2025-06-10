using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Showtime : BaseEntity
{
   
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public Guid? MovieId { get; set; }
    public virtual Movie? Movie { get; set; }

    public virtual ICollection<MovieTicket> MovieTickets { get; set; } = new List<MovieTicket>();


    public Guid? RoomId { get; set; }
    public virtual CinemaRoom? Room { get; set; }
}
