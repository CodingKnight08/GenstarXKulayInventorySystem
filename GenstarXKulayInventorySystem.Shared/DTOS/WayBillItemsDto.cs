namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class WayBillItemsDto:BaseEntityDto
{
    public int Id { get; set; }
    public int WayBillId { get; set; }
    public WayBillDto? WayBill { get; set; }
    public int BranchProductId { get; set; }
    public BranchProductDto? BranchProduct { get; set; }
    public decimal Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
