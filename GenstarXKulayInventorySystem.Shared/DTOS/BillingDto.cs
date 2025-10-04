using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class BillingDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? OperationsProviderId { get; set; }
    public OperationsProviderDto? OperationsProvider { get; set; }
    public int? DailySaleIdId { get; set; }
    public DailySaleReportDto? DailySaleReportDto { get; set; }
    public string BillingName { get; set; } = string.Empty;
    public string BillingNumber { get; set; } = string.Empty;
    public DateTime DateOfBilling { get; set; } = DateTime.Now;
    public BillingCategory Category { get; set; } = BillingCategory.Electric;
    public string? Remarks { get; set; }
    public decimal Amount { get; set; } = 0;
    public bool Discounted { get; set; } = false;
    public decimal? DiscountAmount { get; set; } = 0;
    public bool IsPaid { get; set; } = false;
    public string Data { get; set; } = string.Empty;
    public BillingBranch Branch { get; set; } = BillingBranch.GenStar;
    public DateTime? DatePaid { get; set; } = null;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReceiptNumber { get; set; } = string.Empty;
}
