using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("CollectionProduct")]
[Index("CollectionId", Name = "IX_CollectionProduct_collectionId")]
[Index("ProductId", Name = "IX_CollectionProduct_productId")]
public partial class CollectionProduct : EntityBase
{
    

    [Column("productId")]
    public int? ProductId { get; set; }

    [Column("collectionId")]
    public int? CollectionId { get; set; }

    [ForeignKey("CollectionId")]
    [InverseProperty("CollectionProducts")]
    public virtual Collection? Collection { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("CollectionProducts")]
    public virtual Product? Product { get; set; }
}
