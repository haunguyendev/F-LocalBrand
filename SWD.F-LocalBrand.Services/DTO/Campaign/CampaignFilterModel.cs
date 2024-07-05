using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Campaign
{
    public class CampaignFilterModel
    {
        public string? CampaignName { get; set; }

        public string? Status { get; set; }

        // Thuộc tính sắp xếp
        public string? SortBy { get; set; }
        public bool IsAscending { get; set; } = true;
    }
}
