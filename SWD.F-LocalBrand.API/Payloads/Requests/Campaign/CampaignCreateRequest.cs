using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO.Campaign;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Campaign
{
    public class CampaignCreateRequest
    {
        [Required(ErrorMessage = "Campaign Name is required.")]
        [StringLength(50, ErrorMessage = "Campaign Name must not exceed 50 characters.")]
        public string? CampaignName { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Status(ErrorMessage = "Invalid status.")]
        public string? Status { get; set; }
        public List<int>? CollectionIds { get; set; }

        public CampaignCreateModel MapToModel()
        {
            return new CampaignCreateModel
            {
                CampaignName = this.CampaignName,
                Status = this.Status,
                CollectionIds = this.CollectionIds
                
            };
        }
    }
}
