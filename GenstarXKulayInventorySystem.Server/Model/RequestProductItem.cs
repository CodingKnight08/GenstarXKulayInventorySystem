using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class RequestProductItem:BaseEntity
{
    public int Id { get; set; }
    public int PullOutRequestId { get; set; }
    public PullOutRequest PullOutRequest { get; set; } = null!;
    public int? MasterProductId { get; set; }
    public GlobalProduct? MasterProduct { get; set; } 
    public int? ProductSourceBranchId { get; set; }
    public BranchProduct? ProductSourceBranch { get; set; }
    public int? ProductRequesterBranchId { get; set; }
    public BranchProduct? ProductRequesterBranch { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int ReleasedQuantity { get; set; }
    public bool IsReceived { get; set; }
    public BranchOption Branch { get; set; } 
    public BranchOption SourceProduct { get; set; } = BranchOption.Warehouse;
    public DateTime? DateRecieved { get; set; } 
    public string ProductCode { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public decimal ItemCost { get; set; } = 0;
    public decimal TotalCost { get; set; } = 0;
}
