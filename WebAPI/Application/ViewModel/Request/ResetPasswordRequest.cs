using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.ViewModel.Request
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Password is required")]
        public string? NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Confirmation password does not match")]
        public string? ConfirmPassword { get; set; }
    }
}
