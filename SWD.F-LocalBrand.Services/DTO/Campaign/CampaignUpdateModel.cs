using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Campaign
{
    public class CampaignUpdateModel
    {
        public int Id { get; set; }
        public string? CampaignName { get; set; }
        public List<int>? CollectionIds { get; set; }
    }
}
