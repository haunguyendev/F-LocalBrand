using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Product
{
    public class AddRecommendedProductsRequest
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Recommended Product IDs are required.")]
        public List<int> RecommendedProductIds { get; set; }
    }
}
