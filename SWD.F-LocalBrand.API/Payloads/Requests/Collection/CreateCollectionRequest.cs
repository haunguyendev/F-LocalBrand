using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO.Collection;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Collection
{
    public class CreateCollectionRequest
    {
        [Required(ErrorMessage = "Collection Name is required.")]
        [StringLength(50, ErrorMessage = "Collection Name must not exceed 50 characters.")]
        public string CollectionName { get; set; }
        
        public int? CampaignId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [Status(ErrorMessage = "Invalid status.")]
        public string? Status { get; set; }
        public List<int>? CollectionProductIds { get; set; }
        public CollectionCreateModel MapToModel()
        {
            return new CollectionCreateModel
            {
                CollectionName = this.CollectionName,
                Status = this.Status,
                CampaignId=this.CampaignId,
                CollectionProductIds = this.CollectionProductIds
            };
        }
    }
}
