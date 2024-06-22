using SWD.F_LocalBrand.Business.DTO.Category;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Category
{
    public class CategoryUpdateRequest
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        [StringLength(255, ErrorMessage = "CategoryName must not exceed 255 characters.")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; }

        public CategoryUpdateModel MapToModel()
        {
            return new CategoryUpdateModel
            {
                Id = Id,
                CategoryName = this.CategoryName,
                Description = this.Description
            };
        }
    }
}
