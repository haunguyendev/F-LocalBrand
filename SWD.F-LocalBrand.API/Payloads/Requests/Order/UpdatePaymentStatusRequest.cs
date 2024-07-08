using SWD.F_LocalBrand.Business.Common.Shared;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Order
{
    public class UpdatePaymentStatusRequest
    {
        [Required(ErrorMessage = "PaymentId is required")]
        public int PaymentId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("Completed|Failed|Expired", ErrorMessage = "Invalid order status. Allowed values:Completed, Failed, Expired")]
        public string Status { get; set; }
        [Required(ErrorMessage = "StatusCode is required")]
        public int StatusCode { get; set; }
    }
}
