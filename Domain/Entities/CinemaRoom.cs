using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class CinemaRoom:BaseEntity
{
   

    public Guid? TheaterId { get; set; }

    public string? Name { get; set; }

    public int? Capacity { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    public virtual Theater? Theater { get; set; }
}
