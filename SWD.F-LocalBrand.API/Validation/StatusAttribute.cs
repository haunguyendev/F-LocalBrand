using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Validation
{
    public class StatusAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var status = value as string;
            if (status != null)
            {
                var allowedStatus = new[] { "Active", "Inactive", "Deleted"};
                if (!allowedStatus.Contains(status))
                {
                    return new ValidationResult("Invalid status.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
