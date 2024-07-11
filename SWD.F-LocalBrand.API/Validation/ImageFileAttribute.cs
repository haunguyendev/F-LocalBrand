using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Validation
{
    public class ImageFileAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/jpg", "image/gif", "image/bmp" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    return new ValidationResult("Invalid file type. Only JPEG, PNG, GIF, and BMP are allowed.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
