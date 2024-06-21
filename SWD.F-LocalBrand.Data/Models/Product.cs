using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("Product")]
[Index("CampaignId", Name = "IX_Product_campaignId")]
[Index("CategoryId", Name = "IX_Product_categoryId")]
public partial class Product : EntityBase
{
   

    [Column("productName")]
    [StringLength(255)]
    public string? ProductName { get; set; }

    [Column("categoryId")]
    public int? CategoryId { get; set; }

    [Column("campaignId")]
     public int? CampaignId { get; set; }

    [Column("gender")]
    [StringLength(10)]
    public string? Gender { get; set; }

    [Column("price", TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    [Column("description")]
    [StringLength(500)]
    public string? Description { get; set; }

    [Column("stockQuantity")]
    public int? StockQuantity { get; set; }

    [Column("imageURL")]
    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [Column("size")]
    public int? Size { get; set; }

    [Column("color")]
    [StringLength(20)]
    public string? Color { get; set; }

    [Column("status")]
    [StringLength(30)]
    public string Status { get; set; } = null!;

    [ForeignKey("CampaignId")]
    [InverseProperty("Products")]
    public virtual Campaign? Campaign { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Products")]
    public virtual Category? Category { get; set; }

    [InverseProperty("Product")]
    public virtual ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();

    [InverseProperty("Product")]
    public virtual ICollection<Compapility> CompapilityProducts { get; set; } = new List<Compapility>();

    [InverseProperty("RecommendedProduct")]
    public virtual ICollection<Compapility> CompapilityRecommendedProducts { get; set; } = new List<Compapility>();

    [InverseProperty("Product")]
    public virtual ICollection<CustomerProduct> CustomerProducts { get; set; } = new List<CustomerProduct>();

    [InverseProperty("Product")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
