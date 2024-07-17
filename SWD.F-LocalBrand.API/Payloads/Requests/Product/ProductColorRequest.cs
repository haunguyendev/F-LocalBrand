using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Product
{
    public class ProductColorRequest
    {
        [Required(ErrorMessage = "Color Name is required")]
        public string ColorName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }
    }
}
