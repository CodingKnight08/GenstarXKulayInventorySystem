using GenstarXKulayInventorySystem.Server.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GenstarXKulayInventorySystem.Server;

public class InventoryDbContext: IdentityDbContext<User>
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
       : base(options)
    {
    }

    public InventoryDbContext()
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        

        // Product → ProductBrand (Cascade delete when Brand is deleted)
        modelBuilder.Entity<Product>(entity =>
        {
            // Relationships
            entity.HasOne(p => p.ProductBrand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.ProductCategory)
                .WithMany(c => c.Products) 
                .HasForeignKey(p => p.ProductCategoryId)
                .OnDelete(DeleteBehavior.SetNull);


            // Decimal precision
            entity.Property(p => p.Size).HasPrecision(18, 2);
            entity.Property(p => p.CostPrice).HasPrecision(18, 2);
            entity.Property(p => p.RetailPrice).HasPrecision(18, 2);
            entity.Property(p => p.WholesalePrice).HasPrecision(18, 2);
            entity.Property(p => p.ActualQuantity).HasPrecision(18, 4);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasOne(po => po.Supplier)
                .WithMany()
                .HasForeignKey(po => po.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(po => po.AssumeTotalAmount).HasPrecision(18, 2);

            entity.Property(po => po.PurchaseShipToOption)
                .HasConversion<int>();

            entity.Property(po => po.PurchaseRecieptOption)
                .HasConversion<int>();

            entity.Property(po => po.PurchaseRecieveOption)
                .HasConversion<int>();
        });

        base.OnModelCreating(modelBuilder);
    }



    public DbSet<ProductBrand> ProductBrands { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
}
