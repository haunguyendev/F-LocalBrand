using SWD.F_LocalBrand.Business.DTO.Customer;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Customer
{
    public class CustomerUpdateAccountRequest
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(255, ErrorMessage = "Email length can't be more than 255.")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "FullName length can't be more than 255.")]
        public string? FullName { get; set; }

        public IFormFile? ImageUrl { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(20, ErrorMessage = "Phone length can't be more than 20.")]
        public string? Phone { get; set; }

        [StringLength(500, ErrorMessage = "Address length can't be more than 500.")]
        public string? Address { get; set; }

        public CustomerUpdateModel MapToModel(int customerId)
        {
            return new CustomerUpdateModel
            {
                Id = customerId,
                FullName = FullName,
                Email = Email,
                ImageUrl = ImageUrl,
                Phone = Phone,
                Address = Address
            };
        }
    }
}
