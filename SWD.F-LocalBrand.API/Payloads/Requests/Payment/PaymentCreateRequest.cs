using SWD.F_LocalBrand.Business.DTO.Payment;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Payment
{
    public class PaymentCreateRequest
    {
        [Required(ErrorMessage = "OrderId is required.")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "PaymentDate is required.")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "PaymentMethod is required.")]
        [StringLength(100, ErrorMessage = "PaymentMethod can't be longer than 100 characters.")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "PaymentStatus is required.")]
        [StringLength(50, ErrorMessage = "PaymentStatus can't be longer than 50 characters.")]
        public string PaymentStatus { get; set; }

        public int? StatusResponseCode { get; set; }

        public PaymentCreateModel MapToModel()
        {
            return new PaymentCreateModel
            {
                OrderId = OrderId,
                PaymentDate = PaymentDate,
                PaymentMethod = PaymentMethod,
                PaymentStatus = PaymentStatus,
                StatusResponseCode = StatusResponseCode
            };
        }
    }
}
