    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.IServices;
    using Domain.Entities;
    using Domain.Enums;
    using Application.Common;

    namespace Application.Services
    {
        public class SeatService : a
        {
            private readonly IUnitOfWork _uow;

            public SeatService(IUnitOfWork uow)
            {
                _uow = uow;
            }

            /// <summary>
            /// Lấy toàn bộ ghế trong phòng theo roomId
            /// </summary>
            public async Task<List<Seat>> GetSeatsByRoomAsync(Guid roomId)
            {
                return await _uow.SeatRepo.GetAllAsync(
                    s => s.CinemaRoomId == roomId && s.IsActive);
            }

            /// <summary>
            /// Lấy ghế theo seatId
            /// </summary>
            public async Task<Seat?> GetSeatByIdAsync(Guid seatId)
            {
                return await _uow.SeatRepo.GetAsync(
                    s => s.Id == seatId && s.IsActive);
            }

            /// <summary>
            /// Cập nhật loại ghế cho danh sách seatIds
            /// </summary>
            public async Task UpdateSeatTypeAsync(IEnumerable<Guid> seatIds, SeatTypes newType)
            {
                var seats = await _uow.SeatRepo.GetAllAsync(s => seatIds.Contains(s.Id));
                foreach (var seat in seats)
                {
                    seat.SeatType = newType;
                }
                await _uow.SaveChangesAsync();
            }

            /// <summary>
            /// Cập nhật trạng thái khả dụng (available) cho danh sách ghế
            /// </summary>
            public async Task UpdateSeatAvailabilityAsync(IEnumerable<Guid> seatIds, bool isAvailable)
            {
                var seats = await _uow.SeatRepo.GetAllAsync(s => seatIds.Contains(s.Id));
                foreach (var seat in seats)
                {
                    seat.IsAvailable = isAvailable;
                }
                await _uow.SaveChangesAsync();
            }

        }
    }
