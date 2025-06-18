using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class HoldSeatRequest
    {
        [Required]
        public Guid ShowtimeId { get; set; }
        public List<Guid> SeatIds { get; set; } = new();
    }
}
