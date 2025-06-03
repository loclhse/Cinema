using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Response
{
    public class MemberResponse
    {
        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Identitycart { get; set; }

        public DateOnly? Dob { get; set; }

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }
        public string? Avatar { get; set; }

        public Sex Sex { get; set; }
    }
}
