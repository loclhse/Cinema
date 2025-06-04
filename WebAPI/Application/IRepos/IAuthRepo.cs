using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Common;
using Application.Domain;
using Domain.Entities;

namespace Application.IRepos
{
    /// <summary>
    /// IAuthRepo chỉ định các thao tác liên quan đến user (tạo, lấy, kiểm tra mật khẩu)
    /// bằng DomainUser (thay vì ApplicationUser) và trả về OperationResult (thay vì IdentityResult).
    /// Cũng chứa các thao tác với RefreshToken (Domain.Entities.RefreshToken).
    /// </summary>
    public interface IAuthRepo
    {
        // ----- Phần liên quan đến User (DomainUser) -----

        /// <summary>
        /// Tạo mới một user (đồng thời tạo ApplicationUser phía Infrastructure).
        /// DomainUser chỉ chứa Id, UserName, Email, Phone. Mật khẩu truyền riêng.
        /// Trả về OperationResult: nếu Failed, có thể lấy Errors để hiển thị.
        /// </summary>
        Task<OperationResult> CreateUserAsync(DomainUser user, string password);

        /// <summary>
        /// Gắn role mới cho một user (đồng thời tạo ApplicationUser phía Infrastructure).
        /// Trả về OperationResult: nếu Failed, có thể lấy Errors để hiển thị.
        /// </summary>
        Task<OperationResult> AddUserToRoleAsync(Guid userId, string roleName);


        /// <summary>
        /// Lấy DomainUser + danh sách roles (string[]) theo userName.
        /// Nếu không tìm thấy, trả về (null, empty array).
        /// </summary>
        Task<(DomainUser? user, string[] roles)> GetUserWithRolesAsync(string userName);

        /// <summary>
        /// Lấy DomainUser + danh sách roles (string[]) theo userId.
        /// Nếu không tìm thấy, trả về (null, empty array).
        /// </summary>
        Task<(DomainUser? user, string[] roles)> GetUserWithRolesAsyncById(Guid userId);

        /// <summary>
        /// Kiểm tra mật khẩu (password) với userName cụ thể.
        /// Trả về true nếu đúng, false nếu sai hoặc user không tồn tại.
        /// </summary>
        Task<bool> CheckPasswordAsync(string userName, string password);

        /// <summary>
        /// Kiểm tra mật khẩu (password) với userName cụ thể.
        /// Thay đổi password
        /// </summary>
        Task<OperationResult> ChangePasswordAsync(Guid id, string oldPassword, string newPassword);


        // ----- Phần liên quan đến RefreshToken (Domain.Entities.RefreshToken) -----

        /// <summary>
        /// Thêm một RefreshToken (chưa SaveChanges).
        /// </summary>
        Task AddRefreshTokenAsync(RefreshToken refreshToken);

        /// <summary>
        /// Lấy RefreshToken entity (Domain) theo token string. Nếu không tìm thấy, trả về null.
        /// </summary>
        Task<RefreshToken?> GetRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Thu hồi (revoke) một RefreshToken (chỉ set RevokedAt, chưa SaveChanges).
        /// </summary>
        Task RevokeRefreshTokenAsync(RefreshToken token);

        // Trả về DomainUser (chứa Identity fields), AppUser (profile), cùng danh sách role
        Task<(DomainUser? domainUser, AppUser? appUser, List<string> roles)> GetUserWithRolesAndProfileAsync(string userName);

        // Nếu cần get theo Id:
        Task<(DomainUser? domainUser, AppUser? appUser, List<string> roles)> GetUserWithRolesAndProfileByIdAsync(Guid userId);

        Task<OperationResult> AddBlacklistedTokenAsync(string token);

        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}
