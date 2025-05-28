using System;
using System.Collections.Generic;

namespace Infrastructure.Entities;

public partial class Seat
{
    public int SeatId { get; set; }

    public int? RoomId { get; set; }

    public string? SeatNumber { get; set; }

    public virtual CinemaRoom? Room { get; set; }

    public virtual ICollection<TicketSeat> TicketSeats { get; set; } = new List<TicketSeat>();
}
