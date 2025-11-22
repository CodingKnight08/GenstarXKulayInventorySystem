using System.ComponentModel.DataAnnotations;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class GlobalProductDto:BaseEntityDto
{
    public int Id { get; set; }
    public int? BrandId { get; set; }
    public ProductBrandDto? ProductBrand { get; set; }
    [Required]
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Packaging { get; set; } = string.Empty;
    public List<BranchProductDto> BranchProducts { get; set; } = new List<BranchProductDto>();
}
