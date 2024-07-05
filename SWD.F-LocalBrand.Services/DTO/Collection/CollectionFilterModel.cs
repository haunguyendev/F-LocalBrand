using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Collection
{
    public class CollectionFilterModel
    {
        public string? CollectionName { get; set; }
        public int? CampaignId { get; set; }

        // Thuộc tính sắp xếp
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
