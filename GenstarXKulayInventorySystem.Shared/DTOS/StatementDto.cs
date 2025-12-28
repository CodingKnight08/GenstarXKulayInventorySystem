using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class StatementDto
{
    public string ClientName { get; set; } = string.Empty;
    public DateTime StartRange { get; set; } = DateTime.Now;
    public DateTime EndRange { get; set; } = DateTime.Now;
    public BranchOption Branch { get; set; }
    public List<DailySaleDto>? DailySales { get; set; } = new List<DailySaleDto>();
    public decimal TotalAmount { get; set; }
    public DateTime StatementDate { get; set; }
}
