using System;
using System.Collections.Generic;

namespace Domain.Entities;

public partial class Showtime : BaseEntity
{
    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }
   
    

    public Guid? MovieId { get; set; }
    public virtual Movie? Movie { get; set; }
   
   
    public virtual ICollection<SeatSchedule> SeatSchedules { get; set; } = new List<SeatSchedule>();
    


    
}
