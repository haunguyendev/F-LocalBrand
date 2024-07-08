using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Payment
{
    public class PaymentCreateModel
    {
        public int OrderId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public int? StatusResponseCode { get; set; }
    }
}
