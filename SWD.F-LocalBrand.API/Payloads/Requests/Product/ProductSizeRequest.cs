using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Product
{
    public class ProductSizeRequest
    {
        [Required(ErrorMessage = "Size value is required")]
        public int Size { get; set; }

        [Required(ErrorMessage = "At least one color is required")]
        public List<ProductColorRequest> Colors { get; set; }
    }
}

