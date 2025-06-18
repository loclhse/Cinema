using Application.IServices;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
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

            var expiredSeats = await _unitOfWork.SeatScheduleRepo
                .GetAllAsync(s => s.Status == SeatBookingStatus.Hold && s.HoldUntil <= currentTime);

            foreach (var seat in expiredSeats)
            {
                seat.Status = SeatBookingStatus.Available;
                seat.HoldUntil = null;
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Có thể log ra để kiểm tra nếu cần:
                Console.WriteLine($"[Hangfire] RowVersion conflict: {ex.Message}");
                // Không cần throw lại nếu bạn coi như cleanup nhẹ, lần sau quét tiếp
            }
        }
    }
}
