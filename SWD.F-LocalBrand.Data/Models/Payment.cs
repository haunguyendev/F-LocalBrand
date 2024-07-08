using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("Payment")]
[Index("OrderId", Name = "IX_Payment_orderId")]
public partial class Payment : EntityBase
{
   

    [Column("orderId")]
    public int? OrderId { get; set; }

    [Column("paymentDate")]
    public DateOnly? PaymentDate { get; set; }

    [Column("paymentMethod")]
    [StringLength(100)]
    public string? PaymentMethod { get; set; }

    [Column("paymentStatus")]
    [StringLength(50)]
    public string? PaymentStatus { get; set; }
    [Column("statusResponseCode")]
    public int? StatusResponseCode { get; set; }    

    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual Order? Order { get; set; }
}
