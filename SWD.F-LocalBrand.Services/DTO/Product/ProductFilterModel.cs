using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Product
{
    public class ProductFilterModel
    {
        public string? ProductName { get; set; }
        public int? CategoryId { get; set; }
        public int? CampaignId { get; set; }
        public int? CollectionId { get; set; }
        public string? Gender { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? StockQuantity { get; set; }
        public string? ImageUrl { get; set; }
        public int? Size { get; set; }
        public string? Color { get; set; }
        public string? Status { get; set; }
        public DateTime? CreateDate { get; set; }

        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
