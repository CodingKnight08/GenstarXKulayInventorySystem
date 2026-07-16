using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class StatementReportDocumentDto
{
    public string ClientName { get; set;  } = string.Empty;
    public List<DailySaleDto> DailySales { get; set; } = new List<DailySaleDto>();
    public decimal Balance { get; set; }
    public decimal Credit { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime ReportGenerated { get; set; }
    public BranchOption Branch { get; set;  }
    public string Address { get; set; } = string.Empty;
}
