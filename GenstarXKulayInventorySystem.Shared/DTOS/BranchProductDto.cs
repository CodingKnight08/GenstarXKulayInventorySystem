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
    public decimal WholeSaleCostPrice { get; set; } = 0;
    public int? TiedUpProductId { get; set; }
    public BranchProductDto? TiedUpProduct { get; set; } // ex catalyst

    public int? BasisProductId { get; set; }
    public BranchProductDto? BasisProduct { get; set; } //for future purpose
    public ProductMesurementOption? ProductMesurementOption { get; set; }
}
