using Domain.Enums;

namespace Application.ViewModel
{
    public class WriteEmloyeeAccount
    {
        public string? Password { get; set; }

        public string? Email { get; set; }

        public int? Identitycart { get; set; }

        public DateOnly? Dob { get; set; }

        public string? FullName { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public Sex Sex { get; set; }
    }
}
