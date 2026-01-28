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

            entity.HasOne(p => p.ProductCategory)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.ProductCategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(p => p.Size).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CostPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.RetailPrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.WholesalePrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.ActualQuantity).HasColumnType("decimal(18,2)");
            entity.Property(p => p.BufferStocks).HasColumnType("decimal(18,2)");
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

            entity.Property(po => po.AssumeTotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(po => po.PurchaseShipToOption).HasConversion<int>();
            entity.Property(po => po.PurchaseRecieptOption).HasConversion<int>();
            entity.Property(po => po.PurchaseRecieveOption).HasConversion<int>();
        });

        // PurchaseOrderItem → Product, ProductBrand
        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.HasOne(poi => poi.BranchProduct)
                  .WithMany()
                  .HasForeignKey(poi => poi.BranchProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(poi => poi.ProductBrand)
                  .WithMany()
                  .HasForeignKey(poi => poi.ProductBrandId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(poi => poi.ItemAmount).HasColumnType("decimal(18,2)");
            entity.Property(poi => poi.PurchaseItemMeasurementOption).HasConversion<int>();
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasOne(b => b.DailySaleReport)
                  .WithMany(dsr => dsr.Billings)
                  .HasForeignKey(b => b.DailySaleId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(b => b.OperationsProvider)
                  .WithMany(op => op.Billings)
                  .HasForeignKey(b => b.OperationsProviderId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(b => b.Amount).HasColumnType("decimal(18,2)");
            entity.Property(b => b.DiscountAmount).HasColumnType("decimal(18,2)");
            entity.Property(b => b.Category).HasConversion<int>();
        });

        modelBuilder.Entity<PurchaseOrderBilling>(entity =>
        {
            entity.HasOne(pob => pob.PurchaseOrder)
                  .WithMany(po => po.PurchaseOrderBillings)
                  .HasForeignKey(pob => pob.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(pob => pob.AmountToBePaid).HasColumnType("decimal(18,2)");
            entity.Property(pob => pob.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(pob => pob.DiscountAmount).HasColumnType("decimal(18,2)");

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

            entity.HasOne(ds => ds.DailySaleReport)
                  .WithMany(dsr => dsr.DailySales)
                  .HasForeignKey(ds => ds.DailySaleReportId)
                  .OnDelete(DeleteBehavior.SetNull);
          

            entity.Property(ds => ds.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(ds => ds.Commission).HasColumnType("numeric(18,2)");
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasOne(si => si.DailySale)
                  .WithMany(ds => ds.SaleItems)
                  .HasForeignKey(si => si.DailySaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(si => si.BranchProduct)
                 .WithMany(bp => bp.SaleItems)
                 .HasForeignKey(si => si.BranchProductId)
                 .OnDelete(DeleteBehavior.SetNull);

            // Decimal fields
            entity.Property(si => si.ItemPrice).HasColumnType("decimal(18,2)");
            entity.Property(si => si.Size).HasColumnType("decimal(18,2)");
            entity.Property(si => si.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(si => si.CostPrice).HasColumnType("decimal(18,2)");


            entity.Property(si => si.BranchPurchased)
                  .HasConversion<int>();

            entity.Property(si => si.UnitMeasurement)
                  .HasConversion<int>();

            entity.Property(si => si.ProductPricingOption)
                  .HasConversion<int>();

            entity.Property(si => si.PaintCategory)
                  .HasConversion<int>();
        });

        modelBuilder.Entity<Model.Client>(entity =>
        {
            entity.Property(c => c.ClientName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.Property(c => c.ContactNumber).HasMaxLength(50);
            entity.Property(c => c.CreditBalance).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<DailySaleReport>(entity =>
        {
            entity.Property(dsr => dsr.CashIn).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalSalesToday).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.BeginningBalance).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.InvoiceCash).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.InvoiceChecks).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.NonInvoiceCash).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.NonInvoiceChecks).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalCash).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalChecks).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalSales).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.ChargeSales).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.CollectionCash).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.CollectionChecks).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.Transportation).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.Foods).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.SalaryAndAdvances).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.Commissions).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.Supplies).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.Others).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalExpenses).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalCashOnHand).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.LandedCost).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.GrossProfit).HasColumnType("decimal(18,2)");
        });
        modelBuilder.Entity<GlobalProduct>(entity =>
        {

            modelBuilder.Entity<GlobalProduct>(entity =>
            {
                entity.HasOne(g => g.ProductBrand)
                      .WithMany(b => b.GlobalProducts)      
                      .HasForeignKey(g => g.BrandId)
                      .OnDelete(DeleteBehavior.SetNull);    
            });


            entity.Property(g => g.ProductName)
                  .HasMaxLength(200)
                  .IsRequired();

            entity.Property(g => g.Description)
                  .HasMaxLength(500);

            entity.Property(g => g.Packaging)
                  .HasMaxLength(200);

            entity.HasMany(g => g.BranchProducts)
                  .WithOne(bp => bp.MasterProduct)
                  .HasForeignKey(bp => bp.MasterProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<RequestProductItem>(entity =>
        {
            entity.HasOne(rpi => rpi.MasterProduct)
                  .WithMany(g => g.RequestItems)
                  .HasForeignKey(rpi => rpi.MasterProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            // 🔹 Source Branch Product
            entity.HasOne(rpi => rpi.ProductSourceBranch)
                  .WithMany()
                  .HasForeignKey(rpi => rpi.ProductSourceBranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            // 🔹 Requester Branch Product
            entity.HasOne(rpi => rpi.ProductRequesterBranch)
                  .WithMany()
                  .HasForeignKey(rpi => rpi.ProductRequesterBranchId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(rpi => rpi.Branch)
                  .HasConversion<int>();

            entity.Property(rpi => rpi.SourceProduct)
                  .HasConversion<int>();

            entity.Property(rpi => rpi.ProductName)
                  .HasMaxLength(200)
                  .IsRequired(false);

            entity.Property(rpi => rpi.ProductCode)
                  .HasMaxLength(100);

            entity.Property(rpi => rpi.DateRecieved)
                  .IsRequired(false);

            entity.Property(dsr => dsr.ItemCost).HasColumnType("decimal(18,2)");
            entity.Property(dsr => dsr.TotalCost).HasColumnType("decimal(18,2)");

        });


        modelBuilder.Entity<PullOutRequest>(entity =>
        {
            entity.Property(p => p.Note)
                  .HasMaxLength(500);

            entity.Property(p => p.BranchRequestee)
                  .HasConversion<int>();

            entity.Property(p => p.BranchRequestedTo)
                  .HasConversion<int>();

            entity.HasMany(p => p.RequestProductItems)
                  .WithOne(rpi => rpi.PullOutRequest)
                  .HasForeignKey(rpi => rpi.PullOutRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });



        modelBuilder.Entity<PullOutRequest>(entity =>
        {
            entity.Property(p => p.Note)
                  .HasMaxLength(500);

            entity.Property(p => p.BranchRequestee)
                  .HasConversion<int>();

            entity.Property(p => p.BranchRequestedTo)
                  .HasConversion<int>();

            entity.HasMany(p => p.RequestProductItems)
                  .WithOne(rpi => rpi.PullOutRequest)
                  .HasForeignKey(rpi => rpi.PullOutRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<ReturnItem>(entity =>
        {
            entity.HasOne(ri => ri.DailySale)
                  .WithMany(ds => ds.ReturnItems)
                  .HasForeignKey(ri => ri.DailySaleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ri => ri.BranchProduct)
                  .WithMany()
                  .HasForeignKey(ri => ri.BranchProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.Property(ri => ri.ItemName)
                  .HasMaxLength(200);

            entity.Property(ri => ri.Description)
                  .HasMaxLength(500);

            entity.Property(ri => ri.Size).HasColumnType("decimal(18,2)");
            entity.Property(ri => ri.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(ri => ri.ItemPrice).HasColumnType("decimal(18,2)");
            entity.Property(ri => ri.TotalPrice).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<WayBill>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WayBillNumber)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Courier)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.DateReceived)
                .IsRequired();

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            // Supplier relationship (NO cascade)
            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            // WayBill → WayBillItems (cascade delete)
            entity.HasMany(e => e.WayBillItems)
                .WithOne(wbi => wbi.WayBill)
                .HasForeignKey(wbi => wbi.WayBillId)
                .OnDelete(DeleteBehavior.Restrict);

        
        });



        modelBuilder.Entity<WayBillItems>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ActualQuantity)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ItemPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TotalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.IsMergeToSystem)
                .HasDefaultValue(false);

            // BranchProduct (NO cascade)
            entity.HasOne(e => e.BranchProduct)
                .WithMany()
                .HasForeignKey(e => e.BranchProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // WayBillItem → DamageItems
            entity.HasMany<WayBillDamageItem>()
                .WithOne(d => d.WayBillItem)
                .HasForeignKey(d => d.WayBillItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<WayBillDamageItem>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DamageQuantity)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.DamageAmount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.TotalDamageCost)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);


            // Optional FK to WayBillItem
            entity.HasOne(e => e.WayBillItem)
                .WithMany()
                .HasForeignKey(e => e.WayBillItemId)
                .OnDelete(DeleteBehavior.Cascade);
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
    public DbSet<OperationsProvider> OperationsProviders { get; set; }
    public DbSet<RequestProductItem> RequestProductItems { get; set; }
    public DbSet<PullOutRequest> PullOutRequests { get; set; }
    public DbSet<GlobalProduct> GlobalProducts { get; set; }
    public DbSet<BranchProduct> BranchProducts { get; set; }
    public DbSet<ReturnItem> ReturnItems { get; set; }
    public DbSet<WayBill> WayBills { get; set; }
    public DbSet<WayBillItems> WayBillItems { get; set; }
    public DbSet<WayBillDamageItem> WayBillDamageItems { get; set; }

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
