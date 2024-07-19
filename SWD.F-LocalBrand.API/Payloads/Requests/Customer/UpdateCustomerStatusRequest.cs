using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.Customer
{
    public class UpdateCustomerStatusRequest
    {
        [Required]
        public int CustomerId {  get; set; }
        [Required]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
        public string Status { get; set; }
    }
}
