﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class MovieTimeResponse
    {
       
        public DateOnly Date { get; set; }
        public DateTime StartTime { get; set; }
        public Guid Id { get; set; }
        public Guid CinemaRoomId { get; set; }
        public string? RoomName { get; set; }
    }
}
