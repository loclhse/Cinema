using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Application.Common;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Application.ViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Services
{
    public class CinemaRoomService : ICinemaRoomService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public CinemaRoomService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<CinemaRoomResponse> CreateRoomAsync(CinemaRoomCreateRequest dto)
        {
            var room = _mapper.Map<CinemaRoom>(dto);
            room.Id = Guid.NewGuid();

            var existingRoom = await _uow.CinemaRoomRepo.GetAsync(x => x.Name == room.Name && !x.IsDeleted);
            if (existingRoom != null)
                throw new Exception($"Room with name '{room.Name}' already exists.");

            await _uow.CinemaRoomRepo.AddAsync(room);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CinemaRoomResponse>(room);
        }

        public async Task<CinemaRoomResponse> UpdateRoomAsync(Guid roomId, CinemaRoomUpdateRequest dto)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted);
            if (room == null)
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");

            _mapper.Map(dto, room);
            await _uow.SaveChangesAsync();

            return _mapper.Map<CinemaRoomResponse>(room);
        }

        public async Task<OperationResult> DeleteRoomAsync(Guid roomId)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted);
            if (room == null)
                return OperationResult.Failed(new[] { $"Room with ID {roomId} not found." });

            room.IsDeleted = true;
            await _uow.SaveChangesAsync();

            return OperationResult.Success(new[] { $"Room with ID {roomId} deleted successfully." });
        }

        public async Task<OperationResult> RestoreRoomAsync(Guid roomId)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && x.IsDeleted);
            if (room == null)
                return OperationResult.Failed(new[] { $"Room with ID {roomId} not found or not deleted." });

            room.IsDeleted = false;
            await _uow.SaveChangesAsync();

            return OperationResult.Success(new[] { $"Room with ID {roomId} restored successfully." });
        }

        public async Task<CinemaRoomResponse?> GetRoomByIdAsync(Guid roomId)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted);
            if (room == null)
                return null;

            return _mapper.Map<CinemaRoomResponse>(room);
        }

        public async Task<List<CinemaRoomResponse>> GetAllRoomsAsync(int page, int size)
        {
            var rooms = await _uow.CinemaRoomRepo.GetAllAsync(x => !x.IsDeleted, null, page, size);
            return _mapper.Map<List<CinemaRoomResponse>>(rooms);
        }

        public async Task<CinemaRoomWithSeatsResponse> GetRoomWithSeatsAsync(Guid roomId)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted, include: x => x.Include(r => r.Seats));
            if (room == null)
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");

            return new CinemaRoomWithSeatsResponse
            {
                Id = room.Id,
                Name = room.Name,
                Seats = _mapper.Map<List<SeatResponse>>(room.Seats)
            };
        }

        public async Task<SeatMatrixResponse> GetSeatMatrixAsync(Guid roomId)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted, include: x => x.Include(r => r.Seats));
            if (room == null)
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");

            var matrix = new List<List<SeatResponse>>();
            for (int row = 1; row <= room.TotalRows; row++)
            {
                var rowSeats = room.Seats
                    .Where(s => s.RowIndex == row)
                    .OrderBy(s => s.ColIndex)
                    .Select(s => _mapper.Map<SeatResponse>(s))
                    .ToList();
                matrix.Add(rowSeats);
            }

            return new SeatMatrixResponse
            {
                TotalRows = room.TotalRows,
                TotalCols = room.TotalCols,
                Seats2D = matrix
            };
        }

        public async Task<OperationResult> UpdateLayoutJsonAsync(Guid roomId, JsonDocument newLayout)
        {
            var room = await _uow.CinemaRoomRepo.GetAsync(r => r.Id == roomId)
                       ?? throw new KeyNotFoundException($"CinemaRoom {roomId} does not exist.");

            // Cập nhật layout mới
            room.LayoutJson = newLayout;

            // Thêm bản ghi lịch sử layout (cũng dùng JsonDocument)
            var roomLayout = new RoomLayout
            {
                Id = Guid.NewGuid(),
                CinemaRoomId = roomId,
                LayoutJson = JsonDocument.Parse(newLayout.RootElement.GetRawText())
            };
            await _uow.RoomLayoutRepo.AddAsync(roomLayout);

            await _uow.SaveChangesAsync();
            return OperationResult.Success(new[] { "Layout update successful." });
        }

        // --- Service: CinemaRoomService.cs ---

        public async Task<OperationResult> GenerateSeatsFromLayoutAsync(Guid roomId, JsonDocument newLayout)
        {
            await using var transaction = await _uow.BeginTransactionAsync();

            try
            {
                /* 1. Lấy phòng & cập nhật layout (lưu lịch sử) */
                var room = await _uow.CinemaRoomRepo.GetAsync(r => r.Id == roomId)
                           ?? throw new KeyNotFoundException($"CinemaRoom {roomId} does not exist.");

                var layoutResult = await UpdateLayoutJsonAsync(roomId, newLayout);
                if (!layoutResult.Succeeded)
                    return layoutResult;

                /* 2. Xóa ghế cũ */
                var oldSeats = await _uow.SeatRepo.GetAllAsync(s => s.CinemaRoomId == roomId);
                if (oldSeats.Any())
                    await _uow.SeatRepo.RemoveRangeAsync(oldSeats);

                /* 3. Đọc & validate layout */
                if (!newLayout.RootElement.TryGetProperty("layout", out var layoutElem))
                    return OperationResult.Failed(["LayoutJson is missing a 'layout' field."]);

                if (layoutElem.ValueKind != JsonValueKind.Array || layoutElem.GetArrayLength() == 0)
                    return OperationResult.Failed(["'layout' must be a non-empty 2-D array."]);

                int rowCount = layoutElem.GetArrayLength();
                if (rowCount > room.TotalRows)
                    return OperationResult.Failed([$"Row count ({rowCount}) exceeds room setting ({room.TotalRows})."]);

                var newSeats = new List<Seat>();

                /* 3a. Duyệt từng hàng */
                for (int i = 0; i < rowCount; i++)
                {
                    var rowElem = layoutElem[i];
                    if (rowElem.ValueKind != JsonValueKind.Array)
                        return OperationResult.Failed([$"Row {i + 1} is not an array."]);

                    var cols = rowElem.EnumerateArray().ToList();
                    if (cols.Count > room.TotalCols)
                        return OperationResult.Failed([$"Row {i + 1} has {cols.Count} columns; exceeds room setting ({room.TotalCols})."]);

                    char rowLetter = (char)('A' + i);
                    int seatNo = 1;                           // đếm ghế thực trong hàng

                    for (int j = 0; j < cols.Count; j++)
                    {
                        if (cols[j].ValueKind != JsonValueKind.Number)
                            return OperationResult.Failed([$"Row {i + 1} col {j + 1} is not a number."]);

                        int typeInt = cols[j].GetInt32();
                        if (!Enum.IsDefined(typeof(SeatTypes), typeInt))
                            return OperationResult.Failed([$"SeatType {typeInt} invalid at row {i + 1}, col {j + 1}."]);

                        var type = (SeatTypes)typeInt;

                        /* --- LỐI ĐI --- */
                        if (type == SeatTypes.None)
                        {
                            newSeats.Add(new Seat
                            {
                                Id = Guid.NewGuid(),
                                CinemaRoomId = roomId,
                                RowIndex = i + 1,
                                ColIndex = j + 1,
                                SeatType = SeatTypes.None,
                                IsActive = false,
                                IsAvailable = false,
                                Label = null
                            });
                            continue;  // KHÔNG tăng seatNo
                        }

                        /* --- GHẾ ĐÔI (CoupleLeft) --- */
                        if (type == SeatTypes.CoupleLeft)
                        {
                            if (j == cols.Count - 1)
                                return OperationResult.Failed([$"CoupleLeft at row {i + 1} col {j + 1} has no following CoupleRight."]);

                            var nextInt = cols[j + 1].GetInt32();
                            if ((SeatTypes)nextInt != SeatTypes.CoupleRight)
                                return OperationResult.Failed([$"CoupleLeft at row {i + 1} col {j + 1} but next cell is not CoupleRight."]);

                            var groupId = Guid.NewGuid();

                            // Left seat
                            newSeats.Add(new Seat
                            {
                                Id = Guid.NewGuid(),
                                CinemaRoomId = roomId,
                                RowIndex = i + 1,
                                ColIndex = j + 1,
                                SeatType = SeatTypes.CoupleLeft,
                                Label = $"{rowLetter}{seatNo}",
                                IsActive = true,
                                IsAvailable = true,
                                CoupleGroupId = groupId
                            });

                            // Right seat
                            newSeats.Add(new Seat
                            {
                                Id = Guid.NewGuid(),
                                CinemaRoomId = roomId,
                                RowIndex = i + 1,
                                ColIndex = j + 2,
                                SeatType = SeatTypes.CoupleRight,
                                Label = $"{rowLetter}{seatNo + 1}",
                                IsActive = true,
                                IsAvailable = true,
                                CoupleGroupId = groupId
                            });

                            seatNo += 2;   // ghế đôi chiếm 2 số
                            j++;          // bỏ qua CoupleRight trong vòng lặp
                            continue;
                        }

                        /* --- CoupleRight lẻ --- */
                        if (type == SeatTypes.CoupleRight)
                            return OperationResult.Failed([$"CoupleRight at row {i + 1} col {j + 1} lacks preceding CoupleLeft."]);

                        /* --- GHẾ THƯỜNG & VIP --- */
                        newSeats.Add(new Seat
                        {
                            Id = Guid.NewGuid(),
                            CinemaRoomId = roomId,
                            RowIndex = i + 1,
                            ColIndex = j + 1,
                            SeatType = type,
                            Label = $"{rowLetter}{seatNo}",
                            IsActive = true,
                            IsAvailable = true
                        });

                        seatNo++; // tăng cho ghế đơn
                    }
                }

                /* 4. Lưu DB & commit */
                await _uow.SeatRepo.AddRangeAsync(newSeats);
                await _uow.SaveChangesAsync();
                await transaction.CommitAsync();

                return OperationResult.Success(["Generate seats successfully."]);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return OperationResult.Failed([ex.Message]);
            }
        }


    }
}
