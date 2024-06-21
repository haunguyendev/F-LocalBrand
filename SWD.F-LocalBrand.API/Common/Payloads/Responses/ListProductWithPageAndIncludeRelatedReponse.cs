using SWD.F_LocalBrand.Business.DTO.Product;

namespace SWD.F_LocalBrand.API.Common.Payloads.Responses
{
    public class ListProductWithPageAndIncludeRelatedReponse
    {
        public List<ProductWithAllRelatedModel> Products { get; set; } =null!;
        public int TotalIndex { get; set; }
        public int TotalPage { get;set; }
        public int PageIndex { get;set; }
        
    }
}
