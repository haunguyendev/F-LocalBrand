using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Cart
{
    public class DeleteCartItemRequest
    {
        [Required]
        public int ProductId { get; set; }
    }
}
