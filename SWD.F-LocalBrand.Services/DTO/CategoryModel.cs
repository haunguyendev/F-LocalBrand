using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class CategoryModel
    {
        public int Id { get; set; }

        public string? CategoryName { get; set; }

        public string? Description { get; set; }

        public List<ProductModel> Products { get; set; }
    }
}
