using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ClientDto:BaseEntityDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;
    public BranchOption Branch { get; set; } 
    public List<DailySaleDto> DailySales { get; set; } = new List<DailySaleDto>();

    public string DisplayContactNumber =>
       string.IsNullOrWhiteSpace(ContactNumber)
           ? "N/A"
           : (ContactNumber.StartsWith("+63")
               ? ContactNumber
               : $"+63 {ContactNumber}");
}
