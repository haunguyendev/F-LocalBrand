using SWD.F_LocalBrand.Business.DTO.Customer;
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
        public string CurrentStatus { get; set; }
        public string StatusHistory { get; set; }
        public List<OrderDetailResponseModel> Details { get; set; }
        public CustomerInfoModel CustomerInfo { get; set; }
        
        
    }
}
