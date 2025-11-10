using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class ReturnSale:BaseEntity
{
    public int Id { get; set; }
    public string SalesNumber { get; set; } = string.Empty;
    public int? ProductId { get; set; }
    public DateTime DateOfReturn { get; set; }
    public string NameOfClient { get; set; } = string.Empty;
    public decimal? TotalAmount { get; set; }
    public string ReasonForReturn { get; set; } = string.Empty;
    public BranchOption Branch { get; set; }
    public DailySale? DailySale { get; set; }
    public Product? Product { get; set; }
}
