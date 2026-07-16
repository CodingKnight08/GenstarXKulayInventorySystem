using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class StocksDto
{
    public int MasterProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;

    public decimal BranchQuantity { get; set; }
    public decimal WarehouseQuantity { get; set; }
    public decimal Branch2 { get; set; }

    public decimal? RetailPrice { get; set; }
    public decimal? WholeSalePrice { get; set; }

    public BranchOption Branch { get; set; }
}
