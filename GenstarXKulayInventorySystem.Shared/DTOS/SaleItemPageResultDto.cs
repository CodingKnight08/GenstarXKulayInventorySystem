namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class SaleItemPageResultDto<SaleItemDto>
{
    public List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();
    public int TotalCount { get; set; }
}
