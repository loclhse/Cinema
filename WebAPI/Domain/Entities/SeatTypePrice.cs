using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class SeatTypePrice : BaseEntity
    {
        public SeatTypes SeatType { get; set; } // Enum

        public double DefaultPrice { get; set; }
    }
}
