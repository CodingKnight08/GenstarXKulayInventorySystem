namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class TopSaleItemDto
{
    public string BrandName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public decimal QuantitySold { get; set; }
    public decimal TotalSales { get; set; }
}
