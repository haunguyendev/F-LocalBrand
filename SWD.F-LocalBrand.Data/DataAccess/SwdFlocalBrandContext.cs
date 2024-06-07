using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SWD.F_LocalBrand.Data.Models;

namespace SWD.F_LocalBrand.Data.DataAccess;

public partial class SwdFlocalBrandContext : DbContext
{
    public SwdFlocalBrandContext()
    {
    }

    public SwdFlocalBrandContext(DbContextOptions<SwdFlocalBrandContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<CollectionProduct> CollectionProducts { get; set; }

    public virtual DbSet<Compapility> Compapilities { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerProduct> CustomerProducts { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderHistory> OrderHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Campaign__5447BD4EEC4F03FD");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__23CAF1D84E45E0AA");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Collecti__5BCE195C46E97C24");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Collections).HasConstraintName("FK__Collectio__campa__5535A963");
        });

        modelBuilder.Entity<CollectionProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Collecti__F1E12B3CED625361");

            entity.HasOne(d => d.Collection).WithMany(p => p.CollectionProducts).HasConstraintName("FK__Collectio__colle__59063A47");

            entity.HasOne(d => d.Product).WithMany(p => p.CollectionProducts).HasConstraintName("FK__Collectio__produ__5812160E");
        });

        modelBuilder.Entity<Compapility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compapil__CEB5515FD122782C");

            entity.HasOne(d => d.Product).WithMany(p => p.CompapilityProducts).HasConstraintName("FK__Compapili__produ__4E88ABD4");

            entity.HasOne(d => d.RecommendedProduct).WithMany(p => p.CompapilityRecommendedProducts).HasConstraintName("FK__Compapili__recom__4F7CD00D");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__B611CB7DF00E6088");
        });

        modelBuilder.Entity<CustomerProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__434393BFC41540E3");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerProducts).HasConstraintName("FK__CustomerP__custo__4AB81AF0");

            entity.HasOne(d => d.Product).WithMany(p => p.CustomerProducts).HasConstraintName("FK__CustomerP__produ__4BAC3F29");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__0809335DB78CEB18");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders).HasConstraintName("FK__Order__customerI__412EB0B6");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__E4FEDE4A312C5924");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails).HasConstraintName("FK__OrderDeta__order__440B1D61");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails).HasConstraintName("FK__OrderDeta__produ__44FF419A");
        });

        modelBuilder.Entity<OrderHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderHis__7839F64D1EF70BE8");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderHistories).HasConstraintName("FK__OrderHist__order__52593CB8");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payment__A0D9EFC6AB618159");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments).HasConstraintName("FK__Payment__orderId__47DBAE45");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__2D10D16A0BFF120C");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Products).HasConstraintName("FK__Product__campaig__3C69FB99");

            entity.HasOne(d => d.Category).WithMany(p => p.Products).HasConstraintName("FK__Product__categor__3B75D760");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__760965CC880A1A11");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__CB9A1CFF7CBDB7D0");

            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("FK__User__role_id__5DCAEF64");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
