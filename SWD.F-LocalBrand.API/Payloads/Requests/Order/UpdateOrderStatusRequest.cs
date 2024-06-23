using SWD.F_LocalBrand.Business.DTO.Order;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Order
{
    public class UpdateOrderStatusRequest
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Order status is required.")]
        [RegularExpression("Pending|Processing|Shipped|Delivered|Cancelled|Returned", ErrorMessage = "Invalid order status. Allowed values: Pending, Processing, Shipped, Delivered, Cancelled, Returned")]
        public string OrderStatus { get; set; }

        public UpdateOrderStatusModel MapToModel()
        {
            return new UpdateOrderStatusModel
            {
                Id = Id,
                OrderStatus = OrderStatus
            };
        }
    }
}
