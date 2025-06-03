using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Application.IRepos;
using Application.IServices;
using Domain.Entities;
using Infrastructure.Configuration;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Service
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepo _userRepo;
        private readonly JwtSettings _opt;
        private readonly SigningCredentials _creds;

        public JwtTokenGenerator(
            IOptions<JwtSettings> opt,
            UserManager<ApplicationUser> userManager,
            IUserRepo userRepo)
        {
            _opt = opt.Value;
            _userManager = userManager;
            _userRepo = userRepo;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SecretKey));
            _creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// Sinh JWT dựa vào userId (GUID dạng string) và danh sách roles.
        /// Bên trong, tự fetch thêm email, fullname, phone, v.v. từ Identity + AppUser.
        /// </summary>
        public Task<(string Token, DateTime ExpirationUtc)> GenerateAccessTokenAsync(DomainUser domainUser, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, domainUser.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, domainUser.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, domainUser.Id.ToString())
            };

            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }
            claims.Add(new Claim("role", string.Join(',', roles))); // optional shortcut

            var expires = DateTime.UtcNow.AddMinutes(_opt.AccessTokenExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: _creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult((tokenString, expires));
        }

        /// <summary>
        /// Sinh Refresh Token, gán ngày hết hạn dựa trên _opt.RefreshTokenExpiryDays.
        /// Trả về (Token, ExpirationUtc, Entity) để caller có thể lưu entity vào DB.
        /// </summary>
        public Task<(string Token, DateTime ExpirationUtc, RefreshToken Entity)> GenerateRefreshTokenAsync(Guid userId)
        {
            var refreshTokenValue = Guid.NewGuid().ToString("N");

            var expiration = DateTime.UtcNow.AddDays(_opt.RefreshTokenExpiryDays);

            var tokenEntity = new RefreshToken
            {
                Token = refreshTokenValue,
                ExpiresAt = expiration,
                UserId = userId
            };

            return Task.FromResult((refreshTokenValue, expiration, tokenEntity));
        }
    }
}
