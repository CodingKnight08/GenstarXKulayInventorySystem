using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Shared.DTOS;
public class PullOutRequestDto:BaseEntityDto
{
    public int Id { get; set; }
    public DateTime? DateRequest { get; set; }
    public BranchOption BranchRequestee { get; set; }
    public BranchOption BranchRequestedTo { get; set; }
    public bool Delivered { get; set; }
    public DateTime? DateDelivered { get; set; } = PhilippineTime.Now;
    public string Note { get; set; } = string.Empty;
    public List<RequestProductItemDto> RequestProductItems { get; set; } = new List<RequestProductItemDto>();
}
