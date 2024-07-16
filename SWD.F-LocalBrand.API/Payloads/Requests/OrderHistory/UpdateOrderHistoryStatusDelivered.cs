using SWD.F_LocalBrand.API.Validation;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.OrderHistory
{
    public class UpdateOrderHistoryStatusDelivered
    {
        [Required(ErrorMessage = "Image is required.")]
        [ImageFile(ErrorMessage = "Invalid file type. Only JPEG, PNG, GIF, and BMP are allowed.")]
        public IFormFile ImageUrl { get; set; }
    }
}
