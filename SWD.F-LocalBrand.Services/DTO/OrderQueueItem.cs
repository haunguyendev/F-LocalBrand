using SWD.F_LocalBrand.Business.DTO.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class OrderQueueItem
    {
        public int CustomerId { get; set; }
        public List<CartProductModel> Products { get; set; }
        public string PaymentMethod { get; set; }
    }
}
