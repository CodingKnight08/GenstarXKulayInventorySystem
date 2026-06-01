namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class BranchProductPrice
{
    public int BranchProductId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal? CostPrice { get; set; }

    public decimal? RetailPrice { get; set; }

    public decimal? WholeSalePrice { get; set; }
}
