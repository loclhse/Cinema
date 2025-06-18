using Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Hubs
{
    public class SeatHub : Hub
    { 
        private readonly SeatScheduleService _seatScheduleService;
        public SeatHub(SeatScheduleService seatScheduleService)
        {
            _seatScheduleService = seatScheduleService;
        }
        public async Task HoldSeats(List<Guid> seatIds)
        {
            var heldSeats = await _seatScheduleService.HoldSeatAsync(seatIds);
            await Clients.All.SendAsync("ReceiveHeldSeats", heldSeats);
        }
    }
}
