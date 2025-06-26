using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class CinemaRoomWithSeatsResponse
    {
        public Guid Id { get; set; }               // ID phòng chiếu
        public string Name { get; set; } = string.Empty; // Tên phòng
        public List<SeatResponse> Seats { get; set; } = new(); // Ghế thật đã sinh ra
    }
}
