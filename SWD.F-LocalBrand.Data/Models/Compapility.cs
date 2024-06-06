using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("Compapility")]
[Index("ProductId", Name = "IX_Compapility_productId")]
[Index("RecommendedProductId", Name = "IX_Compapility_recommendedProductId")]
public partial class Compapility : EntityBase
{
   

    [Column("productId")]
    public int? ProductId { get; set; }

    [Column("recommendedProductId")]
    public int? RecommendedProductId { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("CompapilityProducts")]
    public virtual Product? Product { get; set; }

    [ForeignKey("RecommendedProductId")]
    [InverseProperty("CompapilityRecommendedProducts")]
    public virtual Product? RecommendedProduct { get; set; }
}
