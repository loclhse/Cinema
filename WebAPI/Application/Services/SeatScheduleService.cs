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
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<SeatScheduleResponse>> GetHoldSeatByUserIdAsync(Guid showtimeId, Guid userId)
        {
            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(s =>
                s.ShowtimeId == showtimeId &&
                s.Status == SeatBookingStatus.Hold &&
                s.HoldByUserId == userId);

            if (seats == null || !seats.Any())
                return Enumerable.Empty<SeatScheduleResponse>();

            var result = _mapper.Map<List<SeatScheduleResponse>>(seats);

            return result;
        }


        public async Task<IEnumerable<SeatScheduleResponse>> GetSeatSchedulesByShowtimeAsync(Guid showTimeId)
        {
            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(
                x => x.ShowtimeId == showTimeId,
                include: q => q.Include(x => x.Seat!));

            if (seats == null || !seats.Any())
            {
                // Return an empty list if no data is found
                return Enumerable.Empty<SeatScheduleResponse>();
            }

            var result = _mapper.Map<IEnumerable<SeatScheduleResponse>>(seats);
            return result;
        }

        //public async Task<IEnumerable<SeatScheduleResponse>> HoldSeatAsync(List<Guid> seatIds)
        //{
        //    if (seatIds == null || seatIds.Count == 0 || seatIds.Count > 8)
        //        return Enumerable.Empty<SeatScheduleResponse>();

        //    var seats = await _unitOfWork.SeatScheduleRepo
        //        .GetAllAsync(s => seatIds.Contains(s.Id));

        //    foreach (var seat in seats)
        //    {
        //        seat.Status = SeatBookingStatus.Hold;
        //        seat.HoldUntil = DateTime.UtcNow.AddMinutes(5);
        //    }

        //    await _unitOfWork.SaveChangesAsync();
        //    return _mapper.Map<IEnumerable<SeatScheduleResponse>>(seats);
        //}

        public async Task<IEnumerable<SeatScheduleResponse>> HoldSeatAsync(Guid showtimeId, List<Guid> seatIds, Guid userId, string connectionId)
        {
            if (seatIds == null || seatIds.Count is < 1 or > 8)
                return Enumerable.Empty<SeatScheduleResponse>();

            var now = DateTime.UtcNow;

            // Bắt đầu transaction
            await using var tx = await _unitOfWork.BeginTransactionAsync();

            // Lấy các SeatSchedule theo danh sách ghế và suất chiếu
            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(s => s.ShowtimeId == showtimeId && seatIds.Contains(s.Id));

            if (seats == null || !seats.Any())
                return Enumerable.Empty<SeatScheduleResponse>();

            var succeededSeats = new List<SeatSchedule>();

            foreach (var seat in seats)
            {
                var isExpired = seat.Status == SeatBookingStatus.Hold && seat.HoldUntil < now;

                if (seat.Status == SeatBookingStatus.Hold && seat.HoldByUserId != userId && !isExpired)
                {
                    await tx.RollbackAsync();
                    throw new ApplicationException($"Seat {seat.Id} is already held by another user.");
                }

                // Chỉ cho giữ nếu ghế đang available hoặc hold đã hết hạn
                if (seat.Status == SeatBookingStatus.Available || isExpired)
                {
                    seat.Status = SeatBookingStatus.Hold;
                    seat.HoldUntil = now.AddMinutes(5);
                    seat.HoldByUserId = userId;
                    seat.HoldByConnectionId = connectionId;

                    succeededSeats.Add(seat);
                }
            }

            // Nếu không có ghế nào được giữ thành công → không cần save
            if (!succeededSeats.Any())
            {
                await tx.RollbackAsync();
                return Enumerable.Empty<SeatScheduleResponse>();
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();   // vẫn trong transaction
                await tx.CommitAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync();               // có xung đột → huỷ
                throw new InvalidOperationException("Someone hold this/these seat before. Please try again.");
            }

            // Map kết quả trả về
            var result = _mapper.Map<List<SeatScheduleResponse>>(succeededSeats);
            foreach (var item in result)
            {
                item.IsOwnedByCaller = true;
            }

            return result;
        }


        public async Task<OperationResult> UpdateSeatStatusAsync(List<Guid> seatScheduleIds, SeatBookingStatus status)
        {
            if (seatScheduleIds == null || !seatScheduleIds.Any() || seatScheduleIds.Count > 8)
                return OperationResult.Failed(["Seat list must contain between 1 to 8 seats."]);

            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(s => seatScheduleIds.Contains(s.Id));

            if (seats == null || !seats.Any())
                return OperationResult.Failed(["No seat schedules found."]);

            foreach (var seat in seats)
            {
                seat.Status = status;
            }

            await _unitOfWork.SaveChangesAsync();
            return OperationResult.Success([$"{status} successful"]);
        }

        public async Task<OperationResult> ConfirmSeatAsync(List<Guid> seatIds, Guid userId)
        {
            if (seatIds == null || seatIds.Count is < 1 or > 8)
                return OperationResult.Failed(["Seat list must contain 1–8 items."]);

            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(
                    s => seatIds.Contains(s.Id) &&
                         s.Status == SeatBookingStatus.Hold &&
                         s.HoldByUserId == userId);

            if (seats == null || !seats.Any())
                return OperationResult.Failed(["No seat schedules found."]);

            foreach (var seat in seats)
            {
                if (seat.Status != SeatBookingStatus.Hold)
                    return OperationResult.Failed([$"Seat {seat.Id} is not on hold."]);

                seat.Status = SeatBookingStatus.Booked;
                seat.HoldUntil = null;
                seat.HoldByUserId = null;
                seat.HoldByConnectionId = null;
            }

            await _unitOfWork.SaveChangesAsync();
            return OperationResult.Success(["Confirm seats successfully."]);
        }

        public async Task<OperationResult> CancelHoldAsync(List<Guid> seatIds, Guid userId)
        {
            if (seatIds == null || seatIds.Count is < 1 or > 8)
                return OperationResult.Failed(["Seat list must contain 1–8 items."]);

            // Chỉ lấy ghế do chính user hold
            var seats = await _unitOfWork.SeatScheduleRepo.GetAllAsync(
                    s => seatIds.Contains(s.Id) &&
                         s.Status == SeatBookingStatus.Hold &&
                         s.HoldByUserId == userId);

            if (seats == null || !seats.Any())
                return OperationResult.Failed(["No seat schedules found."]);

            foreach (var seat in seats)
            {
                seat.Status = SeatBookingStatus.Available;
                seat.HoldUntil = null;
                seat.HoldByUserId = null;
                seat.HoldByConnectionId = null;
            }

            await _unitOfWork.SaveChangesAsync();
            return OperationResult.Success(["Cancel hold successfully."]);
        }

        public async Task CancelHoldByConnectionAsync(string connectionId, Guid userId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return;

            var seats = await _unitOfWork.SeatScheduleRepo
                .GetAllAsync(s => s.HoldByConnectionId == connectionId &&
                                  s.HoldByUserId == userId &&
                                  s.Status == SeatBookingStatus.Hold);

            if (!seats.Any()) return;

            foreach (var seat in seats)
            {
                seat.Status = SeatBookingStatus.Available;
                seat.HoldUntil = null;
                seat.HoldByUserId = null;
                seat.HoldByConnectionId = null;
            }

            await _unitOfWork.SaveChangesAsync();
        }

    }
}
