using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class ShowtimeResponse
    {
        public Guid Id { get; set; }
        public DateTime StartTime { get; set; }
       
        public int? Duration { get; set; }
        public DateOnly Date { get; set; }
        public DateTime EndTime { get; set; }
        public Guid? CinemaRoomId { get; set; }
        public Guid? MovieId { get; set; }
    }
}
