using SWD.F_LocalBrand.Business.DTO;

namespace SWD.F_LocalBrand.API.Payloads.Responses
{
    public class ListCategoryResponse
    {
        public List<CategoryModel> Categories { get; set; } = null!;
    }
}
