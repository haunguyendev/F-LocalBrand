using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Customer
{
    public class CustomerUpdateModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Image { get; set; }

        public IFormFile? ImageUrl { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
