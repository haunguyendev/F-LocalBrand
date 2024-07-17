using SWD.F_LocalBrand.API.Validation;
using SWD.F_LocalBrand.Business.DTO.Product;
using SWD.F_LocalBrand.Business.DTO;
using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Product
{
    public class CreateProductMultiSizeAndColorRequest
    {
        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(255, ErrorMessage = "Product Name can't be longer than 255 characters")]
        public string ProductName { get; set; }
        [Required(ErrorMessage ="Category is required")]
        public int? CategoryId { get; set; }
        [Required(ErrorMessage ="Campaign is required")]
        public int? CampaignId { get; set; }

        [StringLength(10, ErrorMessage = "Gender can't be longer than 10 characters")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Description can't be longer than 500 characters")]
        public string? Description { get; set; }
        [Required(ErrorMessage ="Image is required")]
        
        public string ImageBase64 { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(30, ErrorMessage = "Status can't be longer than 30 characters")]
        public string Status { get; set; } = null!;

        

        [Required(ErrorMessage = "At least one product size is required")]
        public List<ProductSizeRequest> ProductSizes { get; set; }


        public CreateProductMultiSizeAndColorModel MapToModel()
        {
            return new CreateProductMultiSizeAndColorModel
            {
                ProductName = this.ProductName,
                CategoryId = this.CategoryId,
                CampaignId = this.CampaignId,
                Gender = this.Gender,
                Price = this.Price,
                Description = this.Description,
                ImageBase64 = this.ImageBase64,
                Status = this.Status,
                ProductSizes = this.ProductSizes.Select(size => new ProductSizeModel
                {
                    Size = size.Size,
                    Colors = size.Colors.Select(color => new ProductColorModel
                    {
                        ColorName = color.ColorName,
                        Quantity = color.Quantity
                    }).ToList()
                }).ToList()
            };
        }
    }


}
