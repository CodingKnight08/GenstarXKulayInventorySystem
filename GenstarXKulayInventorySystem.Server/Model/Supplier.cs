using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class Supplier:BaseEntity
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public BranchOption Branch { get; set; } = BranchOption.GeneralSantosCity;
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
