using SWD.F_LocalBrand.Business.DTO;

namespace SWD.F_LocalBrand.API.Common.Payloads.Responses
{
    public class ListOrderResponse
    {
        public List<OrderModel> Orders { get; set; }
    }
}
