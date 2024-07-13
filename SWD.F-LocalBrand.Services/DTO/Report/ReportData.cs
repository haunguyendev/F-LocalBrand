using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Report
{
    public class ReportData
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, int> OrderStatusCounts { get; set; }
        public List<string> TopSellingProducts { get; set; }
        public Dictionary<string, int> PaymentStatusCounts { get; set; }
        public Dictionary<string, int> ShippingStatusCounts { get; set; }
    }
}
