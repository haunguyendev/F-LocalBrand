using System.ComponentModel.DataAnnotations;

namespace SWD.F_LocalBrand.API.Payloads.Requests
{
    public class CategoryDeleteRequest
    {
        [Required(ErrorMessage = "Id is required.")]
        public int Id { get; set; }
    }
}
