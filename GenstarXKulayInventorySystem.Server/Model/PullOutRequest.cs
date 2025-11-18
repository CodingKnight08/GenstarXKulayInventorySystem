using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class PullOutRequest:BaseEntity
{
    public int Id { get; set; }
    public DateTime? DateRequest { get; set; }
    public BranchOption BranchRequestee { get; set; }
    public BranchOption BranchRequestedTo { get; set; }
    public bool Delivered { get; set; }
    public DateTime? DateDelivered { get; set; } = DateTime.UtcNow;
    public string Note { get; set; } = string.Empty;
    public ICollection<RequestProductItem> RequestProductItems { get; set; } = new HashSet<RequestProductItem>();
}
