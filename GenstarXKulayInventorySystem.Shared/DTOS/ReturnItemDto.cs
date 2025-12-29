namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ReturnItemDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? DailySaleId { get; set; }
    public int? BranchProductId { get; set; }
    public BranchProductDto? BranchProduct { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Size { get; set; } = 1;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal ItemPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
public class AddReturnSalesRequest
{
    public int DailySaleId { get; set; }
    public List<ReturnItemDto> ReturnItems { get; set; } = new();
}
