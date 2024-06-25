using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SWD.F_LocalBrand.API.Validation
{
    public class UsernameContainsLetterAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string username = value.ToString();
                if (Regex.IsMatch(username, "[a-zA-Z]"))
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Username must contain at least one letter.");
                }
            }
            return new ValidationResult("Username is required.");
        }
    }
}
