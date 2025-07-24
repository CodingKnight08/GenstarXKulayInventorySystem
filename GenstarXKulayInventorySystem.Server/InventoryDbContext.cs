using GenstarXKulayInventorySystem.Server.Model;
using Microsoft.EntityFrameworkCore;

namespace GenstarXKulayInventorySystem.Server;

public class InventoryDbContext: DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
       : base(options)
    {
    }

    public InventoryDbContext()
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<ProductBrand> ProductBrands { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
}
