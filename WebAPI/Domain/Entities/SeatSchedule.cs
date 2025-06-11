using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class SeatSchedule : BaseEntity
    {
        public Guid? SeatId { get; set; }
        public Guid? ShowtimeId { get; set; }
        public virtual Seat? Seat { get; set; }
        public virtual Showtime? Showtime { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
