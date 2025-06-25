using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class SeatSchedule : BaseEntity
    {
        public Guid SeatId { get; set; }
        public Guid ShowtimeId { get; set; }
        public Guid? OrderId { get; set; }  

        public SeatBookingStatus Status { get; set; } = SeatBookingStatus.Available;

        public virtual Seat? Seat { get; set; }
        public virtual Showtime? Showtime { get; set; }
        public virtual Order? Order { get; set; }

        public DateTime? HoldUntil { get; set; }

        // NEW
        public Guid? HoldByUserId { get; set; }
        public string? HoldByConnectionId { get; set; }

        // -- Row-version để chống race condition (EF Core)
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
