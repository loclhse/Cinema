using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Domain;
using Domain.Entities;

namespace Application.IServices
{
    public interface IJwtTokenGenerator
    {
        Task<(string Token, DateTime ExpirationUtc)> GenerateAccessTokenAsync(DomainUser domainUser, IEnumerable<string> roles);
        Task<(string Token, DateTime ExpirationUtc, RefreshToken Entity)> GenerateRefreshTokenAsync(Guid userId);
    }
}
