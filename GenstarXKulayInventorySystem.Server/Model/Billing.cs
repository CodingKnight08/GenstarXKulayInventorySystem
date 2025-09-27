using System.ComponentModel.DataAnnotations.Schema;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Model;

public class Billing: BaseEntity
{
    public int Id { get; set; }
    public int? DailySaleId { get; set; }
    [ForeignKey(nameof(DailySaleId))]
    public DailySaleReport? DailySaleReport { get; set; }
    public string BillingName { get; set; } = string.Empty;
    public  string BillingNumber { get; set; } = string.Empty;
    public DateTime DateOfBilling { get; set; }
    public BillingCategory Category { get; set; }
    public string? Remarks { get; set; }
    public decimal Amount { get; set; } = 0;
    public bool Discounted { get; set; } = false;
    public decimal? DiscountAmount { get; set; } = 0;
    public string Data { get; set; } = string.Empty;
    public bool IsPaid { get; set; } = false;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public BillingBranch Branch { get; set; } = BillingBranch.GenStar;
    public DateTime? DatePaid { get; set; } = null;
}
