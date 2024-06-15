using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class OrderHistoryModel
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public DateTime? ChangeTime { get; set; }
    }
}
