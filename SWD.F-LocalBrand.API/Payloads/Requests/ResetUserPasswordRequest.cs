using SWD.F_LocalBrand.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests
{
    public class ResetUserPasswordRequest
    {
        [Required(ErrorMessage = "UserName is required.")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, ErrorMessage = "Password can't be longer than 255 characters.")]
        [PasswordComplexity]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
