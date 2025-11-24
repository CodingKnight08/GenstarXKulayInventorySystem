using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;
public class RequestProductItemDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; } //product Id sa requester
    public ProductDto? Product { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int ReleasedQuantity { get; set; }
    public bool IsReceived { get; set; }
    public BranchOption Branch { get; set; }
    public BranchOption SourceProduct { get; set; } = BranchOption.Warehouse;
    public DateTime DateRecieved { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
}

