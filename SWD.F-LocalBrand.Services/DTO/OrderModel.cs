using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class OrderModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public DateOnly? OrderDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? OrderStatus { get; set; }
        public List<OrderHistoryModel>? OrderHistories { get; set; }
    }
}
