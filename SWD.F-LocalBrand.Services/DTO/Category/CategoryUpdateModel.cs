using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Category
{
    public class CategoryUpdateModel
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }

        public string? Description { get; set; }
    }
}
