namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class WayBillItemsDto:BaseEntityDto
{
    public int Id { get; set; }
    public int WayBillId { get; set; }
    public WayBillDto? WayBill { get; set; }
    public int BranchProductId { get; set; }
    public BranchProductDto? BranchProduct { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal ActualQuantity { get; set; }
    public decimal ItemPrice { get; set; } = 0;
    public decimal TotalPrice { get; set; }
}
