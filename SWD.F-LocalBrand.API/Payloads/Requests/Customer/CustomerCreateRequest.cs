using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO.Customer;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Customer
{
    public class CustomerCreateRequest
    {
        [Required(ErrorMessage = "UserName is required.")]
        [StringLength(255, ErrorMessage = "UserName can't be longer than 255 characters.")]
        [UsernameContainsLetter]
        public string UserName { get; set; }

        [Required(ErrorMessage = "FullName is required.")]
        [StringLength(255, ErrorMessage = "FullName can't be longer than 255 characters.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(255, ErrorMessage = "Password can't be longer than 255 characters.")]
        [PasswordComplexity]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(255, ErrorMessage = "Email can't be longer than 255 characters.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone is required.")]
        [StringLength(20, ErrorMessage = "Phone number can't be longer than 20 characters.")]
        [RegularExpression(@"^0[0-9]{9}$", ErrorMessage = "Phone must start with 0 and be 10 digits long.")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }
        public CustomerCreateModel MapToModel()
        {
            return new CustomerCreateModel
            {
                UserName = this.UserName,
                FullName = this.FullName,
                Password = this.Password,
                Email = this.Email,
                Phone = this.Phone,
                Address = this.Address
            };
        }
    }
}
