using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Collection
{
    public class CollectionUpdateModel
    {
        public int Id { get; set; }
        public string? CollectionName { get; set; }
        public int? CampaignId { get; set; }
        public List<int>? CollectionProductIds { get; set; }
    }
}
