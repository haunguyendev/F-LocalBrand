using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("CustomerProduct")]
[Index("CustomerId", Name = "IX_CustomerProduct_customerId")]
[Index("ProductId", Name = "IX_CustomerProduct_productId")]
public partial class CustomerProduct : EntityBase
{
    

    [Column("customerId")]
    public int? CustomerId { get; set; }

    [Column("productId")]
    public int? ProductId { get; set; }

    [Column("buyDate")]
    public DateOnly? BuyDate { get; set; }

    [Column("status")]
    [StringLength(50)]
    public string? Status { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerProducts")]
    public virtual Customer? Customer { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("CustomerProducts")]
    public virtual Product? Product { get; set; }
}
