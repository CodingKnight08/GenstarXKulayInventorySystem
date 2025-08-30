using GenstarXKulayInventorySystem.Shared.Helpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class DailySaleReportDto: BaseEntityDto
{
    public int Id { get; set; }
    public DateTime? Date { get; set; } = UtilitiesHelper.GetPhilippineTime();
    public decimal? CashIn { get; set; }
    public decimal? TotalSalesToday { get; set; }
    public decimal? BeginningBalance { get; set; }
    public decimal InvoiceCash { get; set; }
    public decimal InvoiceChecks { get; set; }
    public decimal NonInvoiceCash { get; set; }
    public decimal NonInvoiceChecks { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalChecks { get; set; }
    public decimal TotalSales { get; set; }
    public decimal? ChargeSales { get; set; }
    public decimal? CollectionCash { get; set; }
    public decimal? CollectionChecks { get; set; }
    public decimal? Transportation { get; set; }
    public decimal? Foods { get; set; }
    public decimal? SalaryAndAdvances { get; set; }
    public decimal? Commissions { get; set; }
    public decimal? Supplies { get; set; }
    public decimal? Others { get; set; }
    public decimal? TotalExpenses { get; set; }
    public decimal? TotalCashOnHand { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string PreparedBy { get; set; } = string.Empty;
    public BranchOption Branch { get; set; }
    public decimal? TotalCollectionCash => CollectionCash;
    public decimal? TotalCollectionChecks => CollectionChecks;
}
