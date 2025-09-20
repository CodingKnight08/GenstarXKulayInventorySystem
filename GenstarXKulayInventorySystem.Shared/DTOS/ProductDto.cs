using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ProductDto:BaseEntityDto
{
    public int Id { get; set; }
    public int BrandId { get; set; }
    public ProductBrandDto ProductBrandDto { get; set; } = new ProductBrandDto();
    public int? ProductCategoryId { get; set; }
    public ProductCategoryDto? ProductCategoryDto { get; set; } 
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; } = 0;
    public decimal? RetailPrice { get; set; } = 0;
    public decimal? WholesalePrice { get; set; }
    public decimal? Size { get; set; }
    public string? Color { get; set; }
    public int Quantity { get; set; } = 0;
    public ProductMesurementOption? ProductMesurementOption { get; set; }
    public BranchOption Branch { get; set; } = BranchOption.GeneralSantosCity;
    public string Packaging { get; set; } = string.Empty;
    public decimal ActualQuantity { get; set; }
    public decimal? BufferStocks { get; set; }
    public string ProductNameAndUnit =>
    string.IsNullOrWhiteSpace(ProductName)
        ? string.Empty
        : $"{ProductName} -({ProductMesurementOption?.ToString()?? ""})";

}
