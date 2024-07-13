using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.OrderHistory
{
    public class UpdateOrderHistoryStatusRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Preparing|Prepared|ShipperReceived|InTransit|Delivered|Cancelled)$", ErrorMessage = "Invalid status.")]
        public string Status { get; set; }
    }
}
