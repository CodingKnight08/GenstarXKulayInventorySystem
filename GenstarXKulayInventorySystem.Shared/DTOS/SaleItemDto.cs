using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class SaleItemDto:BaseEntityDto
{
    public int Id { get; set; }
    public int DailySaleId { get; set; }
    public DailySaleDto? DailySale { get; set; }
    public int? ProductId { get; set; }
    public ProductDto? Product { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal? Size { get; set; }
    public string Description { get; set; } = string.Empty;
    public BranchOption BranchPurchased { get; set; } = BranchOption.GeneralSantosCity;
    public ProductMesurementOption UnitMeasurement { get; set; } = ProductMesurementOption.Gallon;
    public int Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public ProductPricingOption ProductPricingOption { get; set; } = ProductPricingOption.Retail;
    public PaintCategory PaintCategory { get; set; } = PaintCategory.Solid;
    public List<InvolvePaintsDto> DataList { get; set; } = new List<InvolvePaintsDto>();
}

public class InvolvePaintsDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public ProductMesurementOption UnitMeasurement { get; set; }

}