using Application.IServices;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BackgroundService : IBackgroundService
    {
        private readonly IUnitOfWork _unitOfWork;
         public BackgroundService(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }
        public async Task ChangeSeatBookingStatus()
        {
            var currentTime = DateTime.UtcNow;
            var expiredSeats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(s => s.Status == SeatBookingStatus.Hold && s.HoldUntil <= currentTime);
            foreach (var seat in expiredSeats)
            {
                seat.Status = SeatBookingStatus.Available;
                seat.HoldUntil = null;
            }
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
