using System.Text.Json.Serialization;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;
public class RequestProductItemDto:BaseEntityDto
{
    public int Id { get; set; }
    public int PullOutRequestId { get; set; }
    [JsonIgnore]
    public PullOutRequestDto? PullOutRequest { get; set; } = null!;
    public int? MasterProductId { get; set; }
   
    public GlobalProductDto? MasterProduct { get; set; }
    public int? ProductSourceBranchId { get; set; }
    public BranchProductDto? ProductSourceBranch { get; set; }
    public int? ProductRequesterBranchId { get; set; }
    public BranchProductDto? ProductRequesterBranch { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int ReleasedQuantity { get; set; }
    public bool IsReceived { get; set; }
    public BranchOption Branch { get; set; }
    public BranchOption SourceProduct { get; set; } = BranchOption.Warehouse;
    public DateTime DateRecieved { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;

    public decimal ItemCost { get; set; } = 0;
    public decimal TotalCost { get; set; } = 0;
}

