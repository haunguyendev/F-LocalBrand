using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class CompapilityModel
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? RecommendedProductId { get; set; }
        public ProductModel? RecommendedProduct { get; set; }
    }
}
