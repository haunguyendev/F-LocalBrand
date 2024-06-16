using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class CollectionModel
    {
        public int Id { get; set; }
        public string CollectionName { get; set; }
        public List<ProductModel> Products { get; set; }
    }
}
