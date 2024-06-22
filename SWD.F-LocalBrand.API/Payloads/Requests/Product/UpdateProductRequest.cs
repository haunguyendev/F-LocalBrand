using SWD.F_LocalBrand.Business.DTO.Product;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Product
{
    public class UpdateProductRequest
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Product Name must be between 5 and 255 characters.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Category ID is required.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Campaign ID is required.")]
        public int CampaignId { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [RegularExpression("Male|Female", ErrorMessage = "Gender must be 'Male' or 'Female'.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, 9999999999.99, ErrorMessage = "Price must be between 0 and 9999999999.99.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Stock Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be a non-negative number.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(255, ErrorMessage = "Image URL must not exceed 255 characters.")]
        [Url(ErrorMessage = "Image URL must be a valid URL.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Size is required.")]
        public int Size { get; set; }

        [Required(ErrorMessage = "Color is required.")]
        [StringLength(20, ErrorMessage = "Color must not exceed 20 characters.")]
        public string Color { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("Active|Inactive|Deleted", ErrorMessage = "Status must be 'Active', 'Inactive', or 'Deleted'.")]
        public string Status { get; set; } = "Inactive";


        public ProductUpdateModel MapToModel()
        {
            return new ProductUpdateModel
            {
                Id = Id,
                ProductName = ProductName,
                CategoryId = CategoryId,
                CampaignId = CampaignId,
                Gender = Gender,
                Price = Price,
                Description = Description,
                StockQuantity = StockQuantity,
                ImageUrl = ImageUrl,
                Size = Size,
                Color = Color,
                Status = Status
            };
        }
    }
}
