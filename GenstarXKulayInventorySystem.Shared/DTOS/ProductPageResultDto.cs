namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ProductPageResultDto<ProductDto>
{
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    public int TotalCount { get; set; }
}
