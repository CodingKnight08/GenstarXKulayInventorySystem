using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class BranchProduct:BaseEntity
{
    public int Id { get; set; }
    public int? MasterProductId { get; set; }
    public GlobalProduct? MasterProduct { get; set; }
    public BranchOption Branch { get; set; }
    public decimal? CostPrice { get; set; } = 0;
    public decimal? RetailPrice { get; set; } = 0;
    public decimal? WholeSalePrice { get; set; } = 0;
    public decimal? Size { get; set; } = 0;
    public decimal ActualQuantity { get; set; } = 0;
    public decimal BufferStocks { get; set; } = 0;

}
