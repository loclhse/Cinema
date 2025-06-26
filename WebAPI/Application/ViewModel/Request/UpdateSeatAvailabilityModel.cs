using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class UpdateSeatAvailabilityModel
    {
        public IEnumerable<Guid> SeatIds { get; set; } = new List<Guid>();
        public bool IsAvailable { get; set; }
    }
}
