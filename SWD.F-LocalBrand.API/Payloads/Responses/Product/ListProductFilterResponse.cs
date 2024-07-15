using SWD.F_LocalBrand.Business.DTO.Product;

namespace SWD.F_LocalBrand.API.Payloads.Responses.Product
{
    public class ListProductFilterResponse
    {
        public List<ProductWithAllRelatedModel> Products { get; set; } = new List<ProductWithAllRelatedModel>();
    }
}
