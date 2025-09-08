

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class BaseEntityDto
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public string? CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; } = string.Empty;

  
}
