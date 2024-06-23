using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Category
{
    public class CategoryCreateModel
    {
        
        public string? CategoryName { get; set; }

        
        public string? Description { get; set; }
    }
}
