using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Cart
{
    public class CartResponseModel
    {
        public List<CartItemResponseModel> Items { get; set; } = new List<CartItemResponseModel>();
        public decimal TotalCartValue { get; set; }
        public int TotalItems { get; set; }
    }
}
