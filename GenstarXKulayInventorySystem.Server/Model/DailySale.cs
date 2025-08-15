using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Model;

public class DailySale:BaseEntity
{
    public int Id { get; set; }
    public DateTime DateOfSales { get; set; }
    public string NameOfClient { get; set; } = string.Empty;
    public string SalesNumber { get; set; } = string.Empty;
    public string RecieptReference { get; set; } = string.Empty;
    public BranchOption Branch { get; set; } = BranchOption.GeneralSantosCity;
    public PaymentMethod PaymentType { get; set; } = PaymentMethod.Cash;
    public PurchaseRecieptOption SalesOption { get; set; } = PurchaseRecieptOption.NonBIR;
    public decimal? TotalAmount { get; set; } 
    public virtual ICollection<SaleItem> SaleItems { get; set; } = new HashSet<SaleItem>();
}
