using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class SaleItem:BaseEntity
{
    public int Id { get; set; }
    public int DailySaleId { get; set; }
    public DailySale? DailySale { get; set; } 
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Size { get; set; } 
    public string Description { get; set; } = string.Empty;
    public BranchOption BranchPurchased { get; set; } = BranchOption.GeneralSantosCity;
    public ProductMesurementOption UnitMeasurement { get; set; } = ProductMesurementOption.Gallon;
    public int Quantity { get; set; } 
    public decimal ItemPrice { get; set; }
    public ProductPricingOption ProductPricingOption { get; set; } = ProductPricingOption.Retail;
    public PaintCategory PaintCategory { get; set; } = PaintCategory.None;
    public string DataList { get; set; } = string.Empty;

}
