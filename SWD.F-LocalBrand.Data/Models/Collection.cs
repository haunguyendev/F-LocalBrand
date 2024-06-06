using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Index("CampaignId", Name = "IX_Collections_campaignId")]
public partial class Collection : EntityBase
{
   

    [Column("collectionName")]
    [StringLength(255)]
    public string? CollectionName { get; set; }

    [Column("campaignId")]
    public int? CampaignId { get; set; }

    [ForeignKey("CampaignId")]
    [InverseProperty("Collections")]
    public virtual Campaign? Campaign { get; set; }

    [InverseProperty("Collection")]
    public virtual ICollection<CollectionProduct> CollectionProducts { get; set; } = new List<CollectionProduct>();
}
