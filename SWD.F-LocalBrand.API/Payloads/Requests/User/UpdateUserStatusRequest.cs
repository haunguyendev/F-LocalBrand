using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.User
{
    public class UpdateUserStatusRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]       
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Status must be 'Active' or 'Inactive'.")]
        public string Status { get; set; }
    }
}
