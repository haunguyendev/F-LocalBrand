using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class NotificationModel
    {
        public int? CustomerId { get; set; }
        public int? OrderId { get; set; }
        public string? Message { get; set; }
        public string? Status { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
