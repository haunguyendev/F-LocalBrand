namespace SWD.F_LocalBrand.API.Payloads.Requests.Order
{
    public class OrderHistoryStatusRequest
    {
        public string? Status { get; set; }

        public int? CustomerId { get; set; }
    }
}
