using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("OrderHistory")]
[Index("OrderId", Name = "IX_OrderHistory_orderId")]
public partial class OrderHistory : EntityBase
{
    

    [Column("orderId")]
    public int? OrderId { get; set; }

    [Column("status")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("description", TypeName = "text")]
    public string? Description { get; set; }

    [Column("changeTime", TypeName = "datetime")]
    public DateTime? ChangeTime { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderHistories")]
    public virtual Order? Order { get; set; }
}
