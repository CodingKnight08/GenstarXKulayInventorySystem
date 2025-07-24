using System.ComponentModel.DataAnnotations;

namespace GenstarXKulayInventorySystem.Server.Model;

public class ProductBrand:BaseEntity
{
    public int Id { get; set; }

    [Required]
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
}
