using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace WebAPI.Hubs
{
    [Authorize]                       // yêu cầu người dùng đăng nhập
    public class SeatHub : Hub
    {
        private readonly SeatScheduleService _seatService;
        private readonly ILogger<SeatHub> _logger;

        public SeatHub(SeatScheduleService seatService,
                       ILogger<SeatHub> logger)
        {
            _seatService = seatService;
            _logger = logger;
        }

        // ------------------ 1) Giữ ghế (Hold) --------------------------
        // client gọi: connection.invoke("HoldSeats", showtimeId, seatIds)
        public async Task HoldSeats(Guid showtimeId, List<Guid> seatIds)
        {
            try
            {
                if (Context.User == null)
                {
                    await Clients.Caller.SendAsync("Error", "User context is null. Please ensure you are authenticated.");
                    return;
                }

                var userId = Context.User.GetUserId();    // extension dưới
                var heldSeats = await _seatService.HoldSeatAsync(
                                    showtimeId, seatIds, userId, Context.ConnectionId);

                // Gửi kết quả chính cho caller
                await Clients.Caller.SendAsync("HoldResult", heldSeats);

                // Nếu có ghế giữ thành công → broadcast tới nhóm cùng suất
                if (heldSeats.Any())
                {
                    await Clients.Group(showtimeId.ToString())
                                 .SendAsync("ReceiveHeldSeats", heldSeats);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HoldSeats error");
                await Clients.Caller.SendAsync("Error",
                    "Có lỗi xảy ra khi giữ ghế, vui lòng thử lại!");
            }
        }

        // ------------------ 2) Xác nhận đặt (Confirm) ------------------
        public async Task ConfirmSeats(Guid showtimeId, List<Guid> seatIds)
        {
            if (Context.User == null)
            {
                await Clients.Caller.SendAsync("Error", "User context is null. Please ensure you are authenticated.");
                return;
            }

            var result = await _seatService.ConfirmSeatAsync(seatIds, Context.User.GetUserId());

            await Clients.Caller.SendAsync("ConfirmResult", result);

            if (result.Succeeded)
                await Clients.Group(showtimeId.ToString())
                             .SendAsync("ReceiveConfirmedSeats", seatIds);
        }

        // ------------------ 3) Huỷ hold thủ công (Cancel) --------------
        public async Task CancelHold(Guid showtimeId, List<Guid> seatIds)
        {
            if (Context.User == null)
            {
                await Clients.Caller.SendAsync("Error", "User context is null. Please ensure you are authenticated.");
                return;
            }

            await _seatService.CancelHoldAsync(seatIds, Context.User.GetUserId());

            await Clients.Caller.SendAsync("CancelHoldResult", seatIds);
            await Clients.Group(showtimeId.ToString())
                         .SendAsync("ReceiveCancelHold", seatIds);
        }

        // ------------------ 4) Đặt ghế trực tiếp (Skip-Hold) -----------
        // client gọi: connection.invoke("UpdateSeatStatus", showtimeId, seatIds, "Booked")
        public async Task UpdateSeatStatus(Guid showtimeId, List<Guid> seatIds, SeatBookingStatus status)
        {
            if (Context.User == null)
            {
                await Clients.Caller.SendAsync("Error",
                    "User context is null. Please ensure you are authenticated.");
                return;
            }

            // (Tuỳ chọn) Kiểm tra quyền – chỉ cho đặt trực tiếp khi status = Booked
            if (status != SeatBookingStatus.Booked && status != SeatBookingStatus.Available)
            {
                await Clients.Caller.SendAsync("Error",
                    "Unsupported status. Only Booked or Available allowed here.");
                return;
            }

            // Gọi Service
            var result = await _seatService.UpdateSeatStatusAsync(seatIds, status);

            // Trả về riêng cho Caller
            await Clients.Caller.SendAsync("UpdateSeatStatusResult", result);

            // Nếu thành công → Broadcast cho group suất chiếu
            if (result.Succeeded)
            {
                await Clients.Group(showtimeId.ToString())
                             .SendAsync("ReceiveSeatStatusUpdated", new
                             {
                                 SeatIds = seatIds,
                                 NewStatus = status
                             });
            }
            else
            {
                // Thông báo lỗi cụ thể
                await Clients.Caller.SendAsync("Error", string.Join(";", result.Errors));
            }
        }

        // ------------------ 5) Khi client kết nối ----------------------
        public override async Task OnConnectedAsync()
        {
            // Front-end nên gắn ?showtimeId=xxx khi khởi tạo kết nối
            var showtimeIdStr = Context.GetHttpContext()
                                       ?.Request.Query["showtimeId"];

            if (Guid.TryParse(showtimeIdStr, out var showtimeId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, showtimeId.ToString());
            }

            await base.OnConnectedAsync();
        }

        // ------------------ 6) Khi client disconnect -------------------
        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            try
            {
                if (Context.User == null)
                {
                    await Clients.Caller.SendAsync("Error", "User context is null. Please ensure you are authenticated.");
                    return;
                }

                await _seatService.CancelHoldByConnectionAsync(
                        Context.ConnectionId, Context.User.GetUserId());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while auto-releasing seats");
            }

            await base.OnDisconnectedAsync(ex);
        }
    }

    // ---------- Extension: lấy UserId từ ClaimsPrincipal ---------------
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
            => Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
                   ? id
                   : Guid.Empty;
    }
}
