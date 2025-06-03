using Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.DtoResponse
{
    public class UserUpdateResponse
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public int? IdentityCard { get; set; }
        public DateOnly? Dob { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public Sex Sex { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
