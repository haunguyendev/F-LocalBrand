using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Category
{
    public class CategoryFilterModel
    {
        public string? CategoryName { get; set; }
        public string? Description { get; set; }

        // Thuộc tính sắp xếp
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
