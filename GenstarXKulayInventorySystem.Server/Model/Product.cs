using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class Product:BaseEntity
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public ProductBrand ProductBrand { get; set; } = default!;
    public int ProductCategoryId { get; set; }
    [ForeignKey(nameof(ProductCategoryId))]
    public ProductCategory ProductCategory { get; set; } = default!;

    [Required]
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; } = 0;
    public decimal? RetailPrice { get; set; } = 0;
    public decimal? WholesalePrice { get; set; }

    public string? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; } = 0; 
    public BranchOption Branch { get; set; } = BranchOption.GeneralSantosCity;
    public ProductMesurementOption? ProductMesurementOption { get; set; } 
    
}
