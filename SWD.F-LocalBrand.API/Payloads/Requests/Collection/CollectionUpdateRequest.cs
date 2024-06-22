using SWD.F_LocalBrand.Business.DTO.Collection;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Collection
{
    public class CollectionUpdateRequest
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        public string? CollectionName { get; set; }

        public int? CampaignId { get; set; }

        public List<int>? CollectionProductIds { get; set; }

        public CollectionUpdateModel MapToModel()
        {
            return new CollectionUpdateModel
            {
                Id = Id,
                CollectionName = CollectionName,
                CampaignId = CampaignId,
                CollectionProductIds = CollectionProductIds
            };
        }
    }
}
