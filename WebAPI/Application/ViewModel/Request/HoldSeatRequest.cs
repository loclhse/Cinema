using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class HoldSeatRequest
    {
        public List<Guid> SeatIds { get; set; } = new();
    }
}
