using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Order
{
    public class OrderResponseModel
    {
        public int OrderId { get; set; }
        public int? CustomerId { get; set; }
        public DateOnly? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public OrderHistoryResponseModel CurrentHistory { get; set; }
        public OrderDetailResponseModel Details { get; set; }
    }
}
