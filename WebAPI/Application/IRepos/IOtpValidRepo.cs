using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.IRepos
{
    public interface IOtpValidRepo : IGenericRepo<OtpValid>
    {
        /// <summary>
        /// Lấy OTP chưa hết hạn theo userId và mã pin.
        /// </summary>
        Task<OtpValid?> GetValidOtpAsync(Guid userId, string pin);

        /// <summary>
        /// Xoá hết các bản ghi OTP cũ của user (ví dụ khi phát sinh OTP mới).
        /// </summary>
        Task RemoveAllByUserIdAsync(Guid userId);
    }
}
