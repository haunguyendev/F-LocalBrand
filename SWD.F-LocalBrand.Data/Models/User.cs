using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD.F_LocalBrand.Data.Models;

[Table("User")]
[Index("RoleId", Name = "IX_User_role_id")]
public partial class User : EntityBase
{
   

    [Column("userName")]
    [StringLength(255)]
    public string? UserName { get; set; }

    [Column("password")]
    [StringLength(255)]
    public string? Password { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string? Email { get; set; }

    [Column("phone")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("registrationDate")]
    public DateOnly? RegistrationDate { get; set; }

    [Column("image")]
    public string? Image { get; set; }

    [Column("role_id")]
    public int? RoleId { get; set; }

    [ForeignKey("RoleId")]
    [InverseProperty("Users")]
    public virtual Role? Role { get; set; }
}
