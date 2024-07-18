using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Order
{
    public class OrderHistoryResponseModel
    {     
        public string? CurrentStatus { get; set; }
        public DateTime? ChangeTime { get; set; }
    }

}
