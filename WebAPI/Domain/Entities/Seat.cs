using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Seat : BaseEntity
{
    public string? SeatName { get; set; } 

    public Guid? RoomId { get; set; }
    public virtual CinemaRoom? Room { get; set; }
    public bool IsAvailable { get; set; } = true;
    public char? Col{ get; set; }
    public int? Row { get; set; }
    public virtual ICollection<SeatSchedule> SeatSchedules { get; set; } = new List<SeatSchedule>();

    //public virtual ICollection<TicketSeat> TicketSeats { get; set; } = new List<TicketSeat>();
}
