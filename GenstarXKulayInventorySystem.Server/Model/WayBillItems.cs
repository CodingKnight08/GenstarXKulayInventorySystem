namespace GenstarXKulayInventorySystem.Server.Model;

public class WayBillItems:BaseEntity
{
    public int Id { get; set; }
    public int WayBillId { get; set; }
    public WayBill? WayBill { get; set; }
    public int BranchProductId { get; set; }
    public BranchProduct? BranchProduct { get; set; }
    public decimal Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal TotalPrice { get; set; }

    public bool IsMergeToSystem { get; set; }
}
