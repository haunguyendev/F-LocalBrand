using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class CustomerModel
    {
        public int Id { get; set; }

        //public string CustomerName { get; set; } = null!;

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Image { get; set; }

        public DateOnly RegistrationDate { get; set; }

        public List<CustomerProductModel> CustomerProducts { get; set; }
    }
}
