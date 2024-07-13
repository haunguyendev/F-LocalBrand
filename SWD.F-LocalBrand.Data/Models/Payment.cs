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
    [Column("vnp_TxnRef")]
    public string Vnp_TxnRef { get; set; } = string.Empty;
    [Column("vnp_TransactionStatus")]
    public string Vnp_TransactionStatus { get; set; } = string.Empty;
    [Column("vnp_ResponseCode")]
    public string Vnp_ResponseCode { get; set; } = string.Empty;

    [Column("vnp_BankCode")]
    public string Vnp_BankCode { get; set; } = string.Empty;
    [Column("vnp_BankTranNo")]
    public string Vnp_BankTranNo { get; set; } = string.Empty;
    [Column("vnd_CardType")]
    public string Vnd_CardType { get; set; } = string.Empty;




    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual Order? Order { get; set; }
}
