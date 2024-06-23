using SWD.F_LocalBrand.Business.DTO.Campaign;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Campaign
{
    public class CampaignUpdateRequest
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        [StringLength(50, ErrorMessage = "Campaign name must not exceed 50 characters.")]
        public string? CampaignName { get; set; }

        public List<int>? CollectionIds { get; set; }

        public CampaignUpdateModel MapToModel()
        {
            return new CampaignUpdateModel
            {
                Id = Id,
                CampaignName = this.CampaignName,
                CollectionIds = this.CollectionIds
            };
        }
    }
}
