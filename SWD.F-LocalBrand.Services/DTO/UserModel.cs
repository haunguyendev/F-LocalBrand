using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Business.DTO
{
    public class UserModel
    {
        public int UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Image { get; set; } = null!;

        public int RoleId { get; set; }
    }
}
