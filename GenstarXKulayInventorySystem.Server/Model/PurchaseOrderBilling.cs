using GenstarXKulayInventorySystem.Shared.Helpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Model;

public class PurchaseOrderBilling:BaseEntity
{
    public int Id { get; set; }
    public int? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public string PurchaseOrderBillingNumber { get; set; } = string.Empty;
    public DateTime PurchaseOrderBillingDate { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
    public string? Remarks { get; set; }
    public decimal AmountToBePaid { get; set; } = 0;
    public decimal AmountPaid { get; set; } = 0;
    public bool IsPaid { get; set; } = false;
    public BillingBranch BillingBranch { get; set; } = BillingBranch.GenStar;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public bool IsDiscounted { get; set; } = false;
    public decimal? DiscountAmount { get; set; } = 0;
    public PaymentTermsOption PaymentTermsOption { get; set; } = PaymentTermsOption.Today;
    public int? CustomPaymentTermsOption { get; set; } = null;
    public string ItemsRecieved { get; set; } = string.Empty;

}
