using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Application.ViewModel.Request
{
    public class RegisterRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(6, ErrorMessage = "Username must be at least 6 characters.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateOnly? Dob { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? Phone { get; set; }

        [MaxLength(200, ErrorMessage = "Address must not exceed 200 characters.")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Sex is required.")]
        [EnumDataType(typeof(Sex), ErrorMessage = "Invalid value for sex.")]
        public Sex Sex { get; set; }

        [RegularExpression(@"^\d{12}$", ErrorMessage = "Identity card must be exactly 12 digits.")]
        public string? IdentityCard { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = "Guest";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Dob.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (Dob > today)
                {
                    yield return new ValidationResult("Date of birth cannot be in the future.", new[] { nameof(Dob) });
                }

                var age = today.Year - Dob.Value.Year;
                if (Dob.Value.AddYears(age) > today) age--;

                if (age < 18)
                {
                    yield return new ValidationResult("You must be at least 18 years old.", new[] { nameof(Dob) });
                }
            }
        }
    }
}
