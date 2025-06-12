using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Domain.Entities;

namespace Application.IServices
{
    public interface ICinemaRoomService
    {
        Task<CinemaRoomResponse> CreateRoomAsync(CinemaRoomCreateRequest dto);
        Task<CinemaRoomResponse> UpdateRoomAsync(Guid roomId, CinemaRoomUpdateRequest dto);
        Task<OperationResult> DeleteRoomAsync(Guid roomId);                // soft-delete nếu cần
        Task<CinemaRoomResponse?> GetRoomByIdAsync(Guid roomId);
        Task<List<CinemaRoomResponse>> GetAllRoomsAsync(int page, int size);

        /// <summary>Đọc LayoutJson → generate ghế, seed SeatTypeConfig mặc định</summary>
        Task GenerateSeatsFromLayoutAsync(Guid roomId);
    }
}
