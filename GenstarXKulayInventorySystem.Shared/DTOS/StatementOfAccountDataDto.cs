using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class StatementOfAccountDataDto
{
    public string ClientName { get; set; } = string.Empty;
    public decimal BeginningBalance { get; set; }
    public List<DailySaleDto> ChargeSales { get; set; } = new List<DailySaleDto>();
    public decimal TotalRemainingBalance { get; set; } 
    public BranchOption Branch { get; set; }
    public string Address { get; set; } = string.Empty;
    
}
    