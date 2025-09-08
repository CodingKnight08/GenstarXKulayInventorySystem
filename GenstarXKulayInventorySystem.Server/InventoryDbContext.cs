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

            entity.Property(p => p.Size).HasColumnType("numeric(18,2)");
            entity.Property(p => p.CostPrice).HasColumnType("numeric(18,2)");
            entity.Property(p => p.RetailPrice).HasColumnType("numeric(18,2)");
            entity.Property(p => p.WholesalePrice).HasColumnType("numeric(18,2)");
            entity.Property(p => p.ActualQuantity).HasColumnType("numeric(18,2)");
            entity.Property(p => p.BufferStocks).HasColumnType("numeric(18,2)");
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

            entity.Property(po => po.AssumeTotalAmount).HasColumnType("numeric(18,2)");
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

            entity.Property(poi => poi.ItemAmount).HasColumnType("numeric(18,2)");
            entity.Property(poi => poi.PurchaseItemMeasurementOption).HasConversion<int>();
        });

        modelBuilder.Entity<Billing>(entity =>
        {
          
            entity.Property(b => b.Amount).HasColumnType("numeric(18,2)");
            entity.Property(b => b.DiscountAmount).HasColumnType("numeric(18,2)");
            entity.Property(b => b.Category).HasConversion<int>();
        });
        modelBuilder.Entity<PurchaseOrderBilling>(entity =>
        {
            entity.HasOne(pob => pob.PurchaseOrder)
                  .WithMany(po => po.PurchaseOrderBillings)
                  .HasForeignKey(pob => pob.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(pob => pob.AmountToBePaid).HasColumnType("numeric(18,2)");
            entity.Property(pob => pob.AmountPaid).HasColumnType("numeric(18,2)");
            entity.Property(pob => pob.DiscountAmount).HasColumnType("numeric(18,2)");

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

            entity.Property(ds => ds.TotalAmount).HasColumnType("numeric(18,2)");

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
            entity.Property(si => si.ItemPrice).HasColumnType("numeric(18,2)");
            entity.Property(si => si.Size).HasColumnType("numeric(18,2)");

            // Enum conversions
            entity.Property(si => si.BranchPurchased).HasConversion<int>();
            entity.Property(si => si.UnitMeasurement).HasConversion<int>();
            entity.Property(si => si.ProductPricingOption).HasConversion<int>();
            entity.Property(si => si.PaintCategory).HasConversion<int>();
        });
        modelBuilder.Entity<GenstarXKulayInventorySystem.Server.Model.Client>(entity =>
        {
            entity.Property(c => c.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.ContactNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<DailySaleReport>(entity =>
        {
            entity.Property(dsr => dsr.CashIn).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.TotalSalesToday).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.BeginningBalance).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.InvoiceCash).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.InvoiceChecks).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.NonInvoiceCash).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.NonInvoiceChecks).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.TotalCash).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.TotalChecks).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.TotalSales).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.ChargeSales).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.CollectionCash).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.CollectionChecks).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.Transportation).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.Foods).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.SalaryAndAdvances).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.Commissions).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.Supplies).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.Others).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.TotalExpenses).HasColumnType("numeric(18,2)");
            entity.Property(dsr => dsr.TotalCashOnHand).HasColumnType("numeric(18,2)");


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
    public DbSet<GenstarXKulayInventorySystem.Server.Model.Client> Clients { get; set; }
    public DbSet<DailySaleReport> DailySaleReports { get; set; }
    public DbSet<Registration> Registrations { get; set; }

    public static async Task SeedUserAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // 1. Ensure roles exist
        string[] roleNames = { "Admin", "Secratary", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Check if Admin user exists
        var user = await userManager.FindByNameAsync("ITAdministrator");
        if (user == null)
        {
            user = new User
            {
                UserName = "ITAdministrator",
                Email = "genstarkulay@gmail.com",
                EmailConfirmed = true,
                Role = UserRole.Admin,   
                NormalizedUserName="ITADMINISTRATOR",
                Branch = BranchOption.GeneralSantosCity
            };

            // Create user with password
            var result = await userManager.CreateAsync(user, "Administrator@2025");

            if (result.Succeeded)
            {
                // 3. Assign role in AspNetUserRoles
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }

}
