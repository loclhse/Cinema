using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.ViewModel.Response
{
    public class LoginResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateOnly? Dob { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public Sex Sex { get; set; }
        public int? IdentityCard { get; set; }
        public string? Role { get; set; } = "Guest";
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
