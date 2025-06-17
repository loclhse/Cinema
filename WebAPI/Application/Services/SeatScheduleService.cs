using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.IServices;
using Application.ViewModel.Response;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    public class SeatScheduleService : ISeatScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;  
        public SeatScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public Task<SeatScheduleResponse> GetSeatScheduleBySeatIdAsync(Guid seatId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<SeatScheduleResponse>> GetSeatSchedulesByShowtimeAsync(Guid showTimeId)
        {
            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(x => x.ShowtimeId == showTimeId);

            if (seats == null || !seats.Any())
            {
                // Trả về danh sách rỗng nếu không có dữ liệu
                return Enumerable.Empty<SeatScheduleResponse>();
            }

            var result = _mapper.Map<IEnumerable<SeatScheduleResponse>>(seats);
            return result;
        }

        public async Task<IEnumerable<SeatScheduleResponse>> HoldSeatAsync(List<Guid> seatIds)
        {
            if (seatIds == null || seatIds.Count == 0 || seatIds.Count > 8)
                return Enumerable.Empty<SeatScheduleResponse>();

            var seats = await _unitOfWork.SeatScheduleRepo
                .GetAllAsync(s => seatIds.Contains(s.Id));

            foreach (var seat in seats)
            {
                seat.Status = SeatBookingStatus.Hold;
                seat.HoldUntil = DateTime.UtcNow.AddMinutes(5);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<IEnumerable<SeatScheduleResponse>>(seats);
        }

        public async Task<OperationResult> UpdateSeatStatusAsync(List<Guid> seatScheduleIds, SeatBookingStatus status)
        {
            if (seatScheduleIds == null || !seatScheduleIds.Any() || seatScheduleIds.Count > 8)
                return OperationResult.Failed(["Seat list must contain between 1 to 8 seats."]);

            var seats = await _unitOfWork.SeatScheduleRepo
                .GetAllAsync(s => seatScheduleIds.Contains(s.Id));

            if (seats == null || !seats.Any())
                return OperationResult.Failed(["No seat schedules found."]);

            foreach (var seat in seats)
            {
                seat.Status = status;
            }

            await _unitOfWork.SaveChangesAsync();
            return OperationResult.Success([$"{status} successful"]);
        }
    }
}
