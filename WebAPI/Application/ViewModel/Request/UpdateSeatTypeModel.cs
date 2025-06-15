using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.ViewModel.Request
{
    public class UpdateSeatTypeModel
    {
        public IEnumerable<Guid> SeatIds { get; set; } = new List<Guid>();
        public SeatTypes NewType { get; set; }
    }
}
