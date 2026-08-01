namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class DashboardChartDto
{
    public string Label { get; set; } = string.Empty;

    // Total sales revenue
    public decimal NetSales { get; set; }

    // Product cost / COGS
    public decimal LandedCost { get; set; }

    // Operating expenses + purchase expenses
    public decimal Expenses { get; set; }

    // NetSales - LandedCost - Expenses
    public decimal NetProfit { get; set; }

    // Optional for chart tooltip
    public DateTime Date { get; set; }
}
