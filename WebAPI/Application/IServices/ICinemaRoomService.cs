using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Application.Common;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Domain.Entities;

namespace Application.IServices
{
    public interface ICinemaRoomService
    {
        // --- CRUD cơ bản ---
        Task<CinemaRoomResponse> CreateRoomAsync(CinemaRoomCreateRequest dto);
        Task<CinemaRoomResponse> UpdateRoomAsync(Guid roomId, CinemaRoomUpdateRequest dto);
        Task<OperationResult> DeleteRoomAsync(Guid roomId);                // Soft-delete
        Task<OperationResult> RestoreRoomAsync(Guid roomId);              // Optional
        Task<CinemaRoomResponse?> GetRoomByIdAsync(Guid roomId);
        Task<List<CinemaRoomResponse>> GetAllRoomsAsync(int page, int size);

        // --- Hỗ trợ Front-End ---
        Task<CinemaRoomWithSeatsResponse> GetRoomWithSeatsAsync(Guid roomId); // Room + Seats (layout)
        Task<SeatMatrixResponse> GetSeatMatrixAsync(Guid roomId);         // Seats dạng ma trận 2D
        Task<OperationResult> UpdateLayoutJsonAsync(Guid roomId, JsonDocument newLayout); // sửa sơ đồ layout

        // --- Logic nâng cao ---
        Task<OperationResult> GenerateSeatsFromLayoutAsync(Guid roomId, JsonDocument LayoutJson);                   // dùng LayoutJson để seed ghế
    }
}
