using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests.User
{
    public class ChangeUserRoleRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [RegularExpression("^(Admin|Shipper)$", ErrorMessage = "Role of user must be 'Admin' or 'Shipper'.")]
        public string RoleName { get; set; }
    }
}
