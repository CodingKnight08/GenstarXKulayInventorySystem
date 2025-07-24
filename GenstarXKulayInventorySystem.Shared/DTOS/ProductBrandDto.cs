namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ProductBrandDto:BaseEntityDto
{
    public int Id { get; set; }
    
    public string BrandName { get; set; } = string.Empty;
    
    public string? Description { get; set; } = string.Empty;
}
