namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class DailySalePageResultDto<DailySaleDto>
{
    public List<DailySaleDto> Sales { get; set; } = new List<DailySaleDto>();
    public int TotalCount { get; set; }
}
