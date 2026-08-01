namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class DashBoardDto
{
    public decimal TotalProductCosts { get; set; }
    public decimal TotalNetSales { get; set; }
    public decimal Expenses { get; set; }
    public decimal TotalProfit { get; set; } = 0;
    public decimal TotalItemsSold { get; set; }
    public int TotalDailySale { get; set; }
}

public class WeekOptionDto
{
    public string Label { get; set; } = "";
    public DateTime StartDate { get; set; }
}
public class MonthOption
{
    public string Label { get; set; } = "";
    public DateTime Date { get; set; }
}