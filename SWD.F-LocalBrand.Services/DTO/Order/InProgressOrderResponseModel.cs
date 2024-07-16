using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Order
{
    public class InProgressOrderResponseModel
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateOnly OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}
