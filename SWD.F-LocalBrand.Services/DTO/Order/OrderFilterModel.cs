using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Order
{
    public class OrderFilterModel
    {
        public int? CustomerId { get; set; }
        public DateOnly? OrderDate { get; set; }
        public decimal? MinTotalAmount { get; set; }
        public decimal? MaxTotalAmount { get; set; }
        public string? OrderStatus { get; set; }

        // Thuộc tính sắp xếp
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
