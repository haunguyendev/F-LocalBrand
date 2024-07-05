using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO.Category;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Category
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(255, ErrorMessage = "Category Name must not exceed 255 characters.")]
        public string? CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }
        [Required(ErrorMessage = "ImageUrl is required.")]
        [ImageFile(ErrorMessage = "Invalid file type. Only JPEG, PNG, GIF, and BMP are allowed.")]
        public IFormFile ImageUrl { get; set; }

        public CategoryCreateModel MapToModel()
        {
            return new CategoryCreateModel
            {
                CategoryName = this.CategoryName,
                Description = this.Description
            };
        }
    }
}
