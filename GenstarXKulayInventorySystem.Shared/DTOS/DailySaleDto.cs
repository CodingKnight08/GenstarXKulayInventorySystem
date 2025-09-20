using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class DailySaleDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public ClientDto? Client { get; set; }
    public DateTime DateOfSales { get; set; }
    public string NameOfClient { get; set; } = string.Empty;
    public string SalesNumber { get; set; } = string.Empty;
    public string? RecieptReference { get; set; } = string.Empty;
    public BranchOption Branch { get; set; } = BranchOption.GeneralSantosCity;
    public PaymentMethod? PaymentType { get; set; }
    public PaymentTermsOption? PaymentTermsOption { get; set; }
    public int? CustomPaymentTermsOption { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
    public PurchaseRecieptOption? SalesOption { get; set; } 
    public decimal? TotalAmount { get; set; }
    public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
}
