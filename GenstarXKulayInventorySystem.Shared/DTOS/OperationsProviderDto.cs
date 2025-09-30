using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class OperationsProviderDto:BaseEntityDto
{
    public int Id { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string TINNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public BranchOption Branch { get; set; }
    public List<BillingDto> Billings { get; set; } = new List<BillingDto>();
}
