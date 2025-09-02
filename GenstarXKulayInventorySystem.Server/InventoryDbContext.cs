using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

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
            entity.HasOne(ds => ds.Client)
             .WithMany(c => c.DailySales)
             .HasForeignKey(ds => ds.ClientId)
             .OnDelete(DeleteBehavior.SetNull);

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
            entity.Property(si => si.ItemPrice).HasPrecision(18, 2);
            entity.Property(si => si.Size).HasPrecision(18, 2);

            // Enum conversions
            entity.Property(si => si.BranchPurchased).HasConversion<int>();
            entity.Property(si => si.UnitMeasurement).HasConversion<int>();
            entity.Property(si => si.ProductPricingOption).HasConversion<int>();
            entity.Property(si => si.PaintCategory).HasConversion<int>();
        });
        modelBuilder.Entity<Client>(entity =>
        {
            entity.Property(c => c.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.ContactNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<DailySaleReport>(entity =>
        {
            entity.Property(dsr => dsr.CashIn).HasPrecision(18, 2);
            entity.Property(dsr => dsr.TotalSalesToday).HasPrecision(18, 2);
            entity.Property(dsr => dsr.BeginningBalance).HasPrecision(18, 2);
            entity.Property(dsr => dsr.InvoiceCash).HasPrecision(18, 2);
            entity.Property(dsr => dsr.InvoiceChecks).HasPrecision(18, 2);
            entity.Property(dsr => dsr.NonInvoiceCash).HasPrecision(18, 2);
            entity.Property(dsr => dsr.NonInvoiceChecks).HasPrecision(18, 2);
            entity.Property(dsr => dsr.TotalCash).HasPrecision(18, 2);
            entity.Property(dsr => dsr.TotalChecks).HasPrecision(18, 2);
            entity.Property(dsr => dsr.TotalSales).HasPrecision(18, 2);
            entity.Property(dsr => dsr.ChargeSales).HasPrecision(18, 2);
            entity.Property(dsr => dsr.CollectionCash).HasPrecision(18, 2);
            entity.Property(dsr => dsr.CollectionChecks).HasPrecision(18, 2);
            entity.Property(dsr => dsr.Transportation).HasPrecision(18, 2);
            entity.Property(dsr => dsr.Foods).HasPrecision(18, 2);
            entity.Property(dsr => dsr.SalaryAndAdvances).HasPrecision(18, 2);
            entity.Property(dsr => dsr.Commissions).HasPrecision(18, 2);
            entity.Property(dsr => dsr.Supplies).HasPrecision(18, 2);
            entity.Property(dsr => dsr.Others).HasPrecision(18, 2);
            entity.Property(dsr => dsr.TotalExpenses).HasPrecision(18, 2);
            entity.Property(dsr => dsr.TotalCashOnHand).HasPrecision(18, 2);


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
    public DbSet<Client> Clients { get; set; }
    public DbSet<DailySaleReport> DailySaleReports { get; set; }

    public async Task SeedUser()
    {
        User user = new User
        {
            UserName = "ITAdministrator",
            NormalizedUserName = "ITADMINISTRATOR",
            Email = "genstarkulay@gmail.com",
            EmailConfirmed = true,
            LockoutEnabled = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            Role = UserRole.Admin,
            Branch = BranchOption.GeneralSantosCity

        };

        if(!Users.Any(u => u.UserName == user.UserName))
        {
            var password = new PasswordHasher<User>();
            var hashed = password.HashPassword(user, "Administrator@2025");
            user.PasswordHash = hashed;
            await Users.AddAsync(user);
            await SaveChangesAsync();
        }
    }
}
