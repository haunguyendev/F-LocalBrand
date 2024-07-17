using SWD.F_LocalBrand.Business.DTO.Order;

namespace SWD.F_LocalBrand.API.Payloads.Responses.Order
{
    public class ListOrderDetailResponse
    {
        public List<OrderDetailResponseModel> Details { get; set; }
    }
}
