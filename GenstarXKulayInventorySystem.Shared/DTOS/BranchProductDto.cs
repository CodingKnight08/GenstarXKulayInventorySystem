using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class BranchProductDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? MasterProductId { get; set; }
    public GlobalProductDto? MasterProduct { get; set; }
    public BranchOption Branch { get; set; }
    public decimal? CostPrice { get; set; } = 0;
    public decimal? RetailPrice { get; set; } = 0;
    public decimal? WholeSalePrice { get; set; } = 0;
    public decimal? Size { get; set; } = 0;
    public decimal ActualQuantity { get; set; } = 0;
    public decimal BufferStocks { get; set; } = 0;
}
