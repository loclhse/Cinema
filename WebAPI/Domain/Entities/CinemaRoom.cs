using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public partial class CinemaRoom : BaseEntity
{
    public int? TheaterId { get; set; }

    public string? Name { get; set; }

    public int? SeatQuantity { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    public virtual Theater? Theater { get; set; }
    public virtual ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();
}
