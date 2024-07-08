using SWD.F_LocalBrand.Business.DTO.Cart;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Order
{
    public class CreateOrderRequest
    {
        [Required]
        public List<CartProductModel> Products { get; set; }

        [Required]
        [StringLength(100)]
        public string PaymentMethod { get; set; }
    }
}
