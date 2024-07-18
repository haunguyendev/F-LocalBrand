using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO.Customer
{
    public class CustomerInfoModel
    {     
        public string? UserName { get; set; }
        public string? FullName { get; set; }       
        public string? Email { get; set; }   
        public string? Image { get; set; } 
        public string? Phone { get; set; }
        public string? Address { get; set; }       
        public DateOnly? RegistrationDate { get; set; }       
        public string? Status { get; set; }     
        public string? DeviceId { get; set; }
    }
}
