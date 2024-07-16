using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Order
{
    public class OrderHistoryResponseModel
    {
        public int OrderHistoryId { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTime? ChangeTime { get; set; }
    }
}
