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
        // Product → ProductBrand, ProductCategory
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasOne(p => p.ProductBrand)
                  .WithMany(b => b.Products)
                  .HasForeignKey(p => p.BrandId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.ProductCategory)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.ProductCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(p => p.Size).HasPrecision(18, 2);
            entity.Property(p => p.CostPrice).HasPrecision(18, 2);
            entity.Property(p => p.RetailPrice).HasPrecision(18, 2);
            entity.Property(p => p.WholesalePrice).HasPrecision(18, 2);
            entity.Property(p => p.ActualQuantity).HasPrecision(18, 4);
            entity.Property(p => p.BufferStocks).HasPrecision(18, 4);
        });

        // PurchaseOrder → Supplier, PurchaseOrderItems
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasOne(po => po.Supplier)
              .WithMany(s => s.PurchaseOrders)
              .HasForeignKey(po => po.SupplierId)
              .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(po => po.PurchaseOrderItems)
                  .WithOne(poi => poi.PurchaseOrder)
                  .HasForeignKey(poi => poi.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(po => po.AssumeTotalAmount).HasPrecision(18, 2);
            entity.Property(po => po.PurchaseShipToOption).HasConversion<int>();
            entity.Property(po => po.PurchaseRecieptOption).HasConversion<int>();
            entity.Property(po => po.PurchaseRecieveOption).HasConversion<int>();
        });


        // PurchaseOrderItem → Product, ProductBrand
        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasOne(poi => poi.Product)
                  .WithMany()
                  .HasForeignKey(poi => poi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(poi => poi.ProductBrand)
                  .WithMany()
                  .HasForeignKey(poi => poi.ProductBrandId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(poi => poi.ItemAmount).HasPrecision(18, 2);
            entity.Property(poi => poi.PurchaseItemMeasurementOption).HasConversion<int>();
        });

        modelBuilder.Entity<Billing>(entity =>
        {
          
            entity.Property(b => b.Amount).HasPrecision(18, 2);
            entity.Property(b => b.DiscountAmount).HasPrecision(18, 2);
            entity.Property(b => b.Category).HasConversion<int>();
        });
        modelBuilder.Entity<PurchaseOrderBilling>(entity =>
        {
            entity.HasOne(pob => pob.PurchaseOrder)
                  .WithMany(po => po.PurchaseOrderBillings)
                  .HasForeignKey(pob => pob.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(pob => pob.AmountToBePaid).HasPrecision(18, 2);
            entity.Property(pob => pob.AmountPaid).HasPrecision(18, 2);
            entity.Property(pob => pob.DiscountAmount).HasPrecision(18, 2);

            entity.Property(pob => pob.BillingBranch).HasConversion<int>();
            entity.Property(pob => pob.PaymentMethod).HasConversion<int>();
            entity.Property(pob => pob.PaymentTermsOption).HasConversion<int>();
        });

        modelBuilder.Entity<DailySale>(entity =>
        {
            entity.Property(ds => ds.TotalAmount).HasPrecision(18, 2);
        });
        modelBuilder.Entity<SaleItem>(entity =>
        {
            // Relationships
            entity.HasOne(si => si.DailySale)
                  .WithMany(ds => ds.SaleItems) 
                  .HasForeignKey(si => si.DailySaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.Product)                
                   .WithMany(p => p.SaleItems)              
                   .HasForeignKey(si => si.ProductId)       
                   .OnDelete(DeleteBehavior.SetNull);

            // Decimal precision
            entity.Property(si => si.Quantity).HasPrecision(18, 4);
            entity.Property(si => si.ItemPrice).HasPrecision(18, 2);

            // Enum conversions
            entity.Property(si => si.BranchPurchased).HasConversion<int>();
            entity.Property(si => si.UnitMeasurement).HasConversion<int>();
            entity.Property(si => si.ProductPricingOption).HasConversion<int>();
            entity.Property(si => si.PaintCategory).HasConversion<int>();
        });

        base.OnModelCreating(modelBuilder);
    }





    public DbSet<ProductBrand> ProductBrands { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<Billing> Billings { get; set; }
    public DbSet<PurchaseOrderBilling> PurchaseOrderBillings { get; set; }
    public DbSet<DailySale> DailySales { get; set; } 
    public DbSet<SaleItem> SaleItems { get; set; } 
}
