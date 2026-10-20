namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class BranchProductPageResultDto<BranchProductDto>
{
    public List<BranchProductDto> Products { get; set; } = new List<BranchProductDto>();
    public int TotalCount { get; set; }
}
