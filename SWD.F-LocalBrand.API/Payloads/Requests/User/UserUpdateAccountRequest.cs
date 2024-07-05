using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO.User;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.User
{
    public class UserDetailUpdateRequest
    {
        
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(255, ErrorMessage = "Email length can't be more than 255.")]
        public string? Email { get; set; }
        [ImageFile(ErrorMessage = "Invalid file type. Only JPEG, PNG, GIF, and BMP are allowed.")]
        public IFormFile? ImageUrl { get; set; }

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [StringLength(20, ErrorMessage = "Phone length can't be more than 20.")]
        public string? Phone { get; set; }

        [StringLength(200, ErrorMessage = "Address length can't be more than 200.")]
        public string? Address { get; set; }

        public UserUpdateModel MapToModel(int userId)
        {
            return new UserUpdateModel
            {
                Id = userId,
                Email = Email,
                Phone = Phone,
                Address = Address,
                ImageUrl = ImageUrl
            };
        }
    }
}
