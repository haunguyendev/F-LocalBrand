using SWD.F_LocalBrand.Business.DTO.Product;

namespace SWD.F_LocalBrand.API.Common.Payloads.Responses
{
    public class ListProductWithPageAndIncludeRelatedReponse
    {
        public List<ProductWithAllRelatedModel> Products { get; set; } =null!;
    }
}
