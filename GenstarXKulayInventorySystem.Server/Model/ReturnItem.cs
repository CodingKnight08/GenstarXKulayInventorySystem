namespace GenstarXKulayInventorySystem.Server.Model;

public class ReturnItem:BaseEntity
{
    public int Id { get; set; }
    public int? DailySaleId { get; set; }
    public DailySale? DailySale { get; set; }
    public int? BranchProductId { get; set; }
    public BranchProduct? BranchProduct { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Size { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
