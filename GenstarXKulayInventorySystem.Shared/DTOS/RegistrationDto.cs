

using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class RegistrationDto:BaseEntityDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ContactNumber { get; set; } = string.Empty;
    public BranchOption Branch { get; set; } = BranchOption.Warehouse;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
}
public class LoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}