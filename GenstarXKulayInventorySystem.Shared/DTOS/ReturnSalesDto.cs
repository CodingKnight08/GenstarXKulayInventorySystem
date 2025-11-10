using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ReturnSalesDto: BaseEntityDto
{
    public int Id { get; set; }
    public string SalesNumber { get; set; } = string.Empty;
    public DateTime DateOfReturn { get; set; }
    public string NameOfClient { get; set; } = string.Empty;
    public decimal? TotalAmount { get; set; }
    public string ReasonForReturn { get; set; } = string.Empty;
    public BranchOption Branch { get; set; }
    public DailySaleDto? DailySale { get; set; }
}
