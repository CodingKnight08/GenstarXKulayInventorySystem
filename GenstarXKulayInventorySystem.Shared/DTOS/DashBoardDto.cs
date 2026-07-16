namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class DashBoardDto
{
    public decimal TotalProductCosts { get; set; }
    public decimal TotalNetSales { get; set; }
    public decimal Expenses { get; set; }
    public decimal TotalProfit { get; set; } = 0;
}
