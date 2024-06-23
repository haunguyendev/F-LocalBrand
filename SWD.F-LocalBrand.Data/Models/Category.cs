using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("Category")]
public partial class Category : EntityBase
{
    

    [Column("categoryName")]
    [StringLength(255)]
    public string? CategoryName { get; set; }

    [Column("description")]
    public string? Description { get; set; }
    [Column("status")]
    public string? Status { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
