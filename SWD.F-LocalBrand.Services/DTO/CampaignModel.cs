using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class CampaignModel
    {
        public int Id { get; set; }
        public string CampaignName { get; set; }
        public List<CollectionModel> Collections { get; set; }

        public List<ProductModel> Products { get; set; }
    }
}
