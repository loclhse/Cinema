using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.IServices;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Application.ViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            try
            {
                var room = _mapper.Map<CinemaRoom>(dto);
                room.Id = Guid.NewGuid(); // Nếu bạn muốn tự sinh ID, giữ dòng này

                var existingRoom = await _uow.CinemaRoomRepo.GetAsync(x => x.Name == room.Name && !x.IsDeleted);
                if (existingRoom != null)
                {
                    throw new Exception($"Room with name '{room.Name}' already exists.");
                }

                await _uow.CinemaRoomRepo.AddAsync(room);
                await _uow.SaveChangesAsync();

                var roomResponse = _mapper.Map<CinemaRoomResponse>(room);
                return roomResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating room: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteRoomAsync(Guid roomId)
        {
            try
            {
                var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted);
                if (room == null)
                {
                    return OperationResult.Failed(new[] { $"Room with ID {roomId} not found." });
                }

                room.IsDeleted = true;
                await _uow.SaveChangesAsync();

                return OperationResult.Success([$"Room with ID {roomId} deleted successfully."]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting room: {ex.Message}");
            }
        }

        public Task GenerateSeatsFromLayoutAsync(Guid roomId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CinemaRoomResponse>> GetAllRoomsAsync(int page, int size)
        {
            var allRooms = await _uow.CinemaRoomRepo.GetAllAsync(x => !x.IsDeleted, null, page, size);
            var allRoomsResponse = _mapper.Map<List<CinemaRoomResponse>>(allRooms);
            return allRoomsResponse;
        }

        public async Task<CinemaRoomResponse?> GetRoomByIdAsync(Guid roomId)
        {
            try
            {
                var room = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted);
                if (room == null)
                {
                    throw new KeyNotFoundException($"Room with ID {roomId} not found.");
                }
                var roomResponse = _mapper.Map<CinemaRoomResponse>(room);
                return roomResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving room: {ex.Message}");
            }
        }

        public async Task<CinemaRoomResponse> UpdateRoomAsync(Guid roomId, CinemaRoomUpdateRequest dto)
        {
            var existingRoom = await _uow.CinemaRoomRepo.GetAsync(x => x.Id == roomId && !x.IsDeleted);
            if (existingRoom == null)
            {
                throw new KeyNotFoundException($"Room with ID {roomId} not found.");
            }

            _mapper.Map(dto, existingRoom);
            await _uow.SaveChangesAsync();
            var roomResponse = _mapper.Map<CinemaRoomResponse>(existingRoom);
            return roomResponse;
        }

    }
}
