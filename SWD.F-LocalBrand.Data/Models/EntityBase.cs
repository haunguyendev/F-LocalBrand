using SWD.F_LocalBrand.Data.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWD.F_LocalBrand.Data.Models
{
    public abstract class EntityBase : IEntityBase
    {
        [Key]
        public int Id { get; set; }
    }
}
