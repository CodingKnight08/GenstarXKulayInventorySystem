namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ProductCsvDto
{
    public string ProductMeasurementOption { get; set; } = string.Empty;
    public decimal Size { get; set; }
    public decimal SUPPLIERS_PRICE { get; set; }
    public decimal FREIGHT { get; set; }
    public decimal CostPrice { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal WholesalePrice { get; set; }
    public int ProductBrandId { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal BufferStocks { get; set; }
    public int BranchId { get; set; }
    public int Quantity { get; set; }
}
