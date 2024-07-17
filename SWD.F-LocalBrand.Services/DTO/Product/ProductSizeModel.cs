using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Product
{
    public class ProductSizeModel
    {
        public int Size{ get; set; }
        public List<ProductColorModel> Colors { get; set; }
    }
}
