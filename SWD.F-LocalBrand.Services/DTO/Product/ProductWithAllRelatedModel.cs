using SWD.F_LocalBrand.Business.DTO.Campaign;
using SWD.F_LocalBrand.Business.DTO.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Product
{
    public class ProductWithAllRelatedModel
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }

        public string? SubCategory { get; set; }

        public string? Gender { get; set; }

        public decimal? Price { get; set; }

        public string? Description { get; set; }

        public int? StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public int? Size { get; set; }

        public string? Color { get; set; }
        public string Status { get; set; } = null!;

        public CampaignWithInfoModel Campaign { get; set; }

        public CategoryWithInfoModel Category { get; set; }
        public List<ProductWithInfoModel> ProductsRecommendation { get; set; }


    }
}
