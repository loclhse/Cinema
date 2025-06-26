using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
   public class ShowtimeUpdateRequest
    {
        public DateTime StartTime { get; set; }
        public DateOnly Date { get; set; }
        public Guid CinemaRoomId { get; set; }
        public Guid MovieId { get; set; }
    }
}
