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
        public DateTime? StartTime { get; set; } 
        public DateTime? EndTime { get; set; }
       

        public ShowtimeResponse(Showtime showtime)
        {
            Id = showtime.Id;
            StartTime = showtime.StartTime;
            EndTime = showtime.EndTime;
           
        }
    }
}
