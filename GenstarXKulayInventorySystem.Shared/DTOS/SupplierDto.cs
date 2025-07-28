namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class SupplierDto:BaseEntityDto
{
    public int Id { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? ContactNumber { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
}
