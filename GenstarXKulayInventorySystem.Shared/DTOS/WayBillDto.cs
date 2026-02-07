using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.WalBillHelper;

namespace GenstarXKulayInventorySystem.Shared.DTOS;
public class WayBillDto:BaseEntityDto
{
    public int Id { get; set; }
    public string WayBillNumber { get; set; } = string.Empty;
    public string Courier { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public SupplierDto? Supplier { get; set; }
    public DateTime DateReceived { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public BranchOption Branch { get; set; }
    public List<WayBillItemsDto> WayBillItems { get; set; } = new List<WayBillItemsDto>();
    public WayBillTermsOption WayBillTerms { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? ExpectedPaymentDate { get; set; }
}
