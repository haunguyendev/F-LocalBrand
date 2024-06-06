using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("Campaign")]
public partial class Campaign :EntityBase
    
{
   

    [Column("campaignName")]
    [StringLength(255)]
    public string? CampaignName { get; set; }

    [InverseProperty("Campaign")]
    public virtual ICollection<Collection> Collections { get; set; } = new List<Collection>();

    [InverseProperty("Campaign")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
