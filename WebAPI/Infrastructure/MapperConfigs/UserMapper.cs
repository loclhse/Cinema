using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Application.ViewModel.Request;
using Application.ViewModel.Response;
using Domain.Entities;

namespace Infrastructure.MapperConfigs
{
    public static class UserMapper
    {
        /// <summary>
        /// Chuyển từ RegisterRequest → DomainUser (chỉ lưu Identity fields)
        /// </summary>
        public static DomainUser ToDomainUser(this RegisterRequest req, Guid newUserId)
        {
            return new DomainUser
            {
                Id = newUserId,
                UserName = req.UserName,
                Email = req.Email,
                Phone = req.Phone
            };
        }

        /// <summary>
        /// Chuyển từ RegisterRequest → AppUser (lưu business fields)
        /// </summary>
        public static AppUser ToAppUser(this RegisterRequest req, Guid newUserId)
        {
            return new AppUser
            {
                Id = newUserId,
                FullName = req.FullName,
                Dob = req.Dob,
                Phone = req.Phone,
                Email = req.Email,
                Address = req.Address,
                Sex = req.Sex,
                IdentityCard = req.IdentityCard
            };
        }

        /// <summary>
        /// Tạo RegisterResponse dựa trên DomainUser + AppUser + role
        /// </summary>
        public static RegisterResponse ToRegisterResponse(
            this DomainUser domainUser,
            AppUser appUser,
            string role)
        {
            return new RegisterResponse
            {
                UserName = domainUser.UserName,
                FullName = appUser.FullName ?? string.Empty,
                Email = domainUser.Email,
                Dob = appUser.Dob,
                Phone = appUser.Phone,
                Address = appUser.Address,
                Sex = appUser.Sex,
                IdentityCard = appUser.IdentityCard,
                Role = role
            };
        }

        /// <summary>
        /// Tạo LoginResponse dựa trên DomainUser + AppUser + role + tokens
        /// </summary>
        public static LoginResponse ToLoginResponse(
            this DomainUser domainUser,
            AppUser appUser,
            string role,
            string accessToken,
            string refreshToken,
            DateTime refreshTokenExpiryTime)
        {
            return new LoginResponse
            {
                Id = domainUser.Id,
                FullName = appUser.FullName ?? string.Empty,
                Email = domainUser.Email,
                Dob = appUser.Dob,
                Phone = appUser.Phone,
                Address = appUser.Address,
                Sex = appUser.Sex,
                IdentityCard = appUser.IdentityCard,
                Role = role,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = refreshTokenExpiryTime
            };
        }
    }
}
