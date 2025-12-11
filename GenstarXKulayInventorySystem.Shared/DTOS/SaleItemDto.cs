using System.Text.Json.Serialization;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class SaleItemDto:BaseEntityDto
{
    public int Id { get; set; }
    public int DailySaleId { get; set; }
    [JsonIgnore]
    public DailySaleDto? DailySale { get; set; }
    public int? BranchProductId { get; set; }
    public BranchProductDto? BranchProduct { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal? Size { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public BranchOption BranchPurchased { get; set; } = BranchOption.GeneralSantosCity;
    public ProductMesurementOption UnitMeasurement { get; set; } = ProductMesurementOption.Gallon;
    public decimal Quantity { get; set; } = 1;
    public decimal ItemPrice { get; set; }
    public ProductPricingOption ProductPricingOption { get; set; } = ProductPricingOption.Retail;
    public PaintCategory PaintCategory { get; set; } = PaintCategory.Solid;
    public List<InvolvePaintsDto> DataList { get; set; } = new List<InvolvePaintsDto>();
    public bool IsDeducted { get; set; } = false;
    public decimal TotalPrice { get; set; } = 0;
    public decimal CostPrice { get; set; }
    public bool HasDiscount { get; set; } = false;
}

public class InvolvePaintsDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal? Size { get; set; } = 1;
    public ProductMesurementOption UnitMeasurement { get; set; }
    public decimal ProductCost { get; set; }
    public ProductMesurementOption ProductUnit { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal CostPrice { get; set; }
}