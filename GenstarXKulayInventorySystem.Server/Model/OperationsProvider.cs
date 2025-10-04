using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class OperationsProvider:BaseEntity
{
    public int Id {get;set; }
    public string ProviderName{ get; set; } = string.Empty;
    public string TINNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public BranchOption Branch { get; set; }
    public ICollection<Billing> Billings { get; set; } = new List<Billing>();
}
