namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class ProductCategoryDto:BaseEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
