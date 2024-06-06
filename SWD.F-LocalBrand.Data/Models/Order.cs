using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("Order")]
[Index("CustomerId", Name = "IX_Order_customerId")]
public partial class Order : EntityBase
{
 

    [Column("customerId")]
    public int? CustomerId { get; set; }

    [Column("orderDate")]
    public DateOnly? OrderDate { get; set; }

    [Column("totalAmount", TypeName = "decimal(10, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column("orderStatus")]
    [StringLength(50)]
    public string? OrderStatus { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Orders")]
    public virtual Customer? Customer { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();

    [InverseProperty("Order")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
