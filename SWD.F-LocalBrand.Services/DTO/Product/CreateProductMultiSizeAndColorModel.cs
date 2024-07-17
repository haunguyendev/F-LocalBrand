using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Product
{
    public class CreateProductMultiSizeAndColorModel
    {
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public int? CampaignId { get; set; }
        public string? Gender { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageBase64 { get; set; }
        public string Status { get; set; }
        public DateTime? CreateDate { get; set; }
        public List<ProductSizeModel> ProductSizes { get; set; }
    }
}
