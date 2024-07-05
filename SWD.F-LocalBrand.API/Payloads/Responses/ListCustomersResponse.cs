using SWD.F_LocalBrand.Business.DTO;

namespace SWD.F_LocalBrand.API.Payloads.Responses
{
    public class ListCustomersResponse
    {
        public List<CustomerModel> Customers { get; set; }
    }
}
