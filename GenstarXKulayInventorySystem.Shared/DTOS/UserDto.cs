

using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class UserDto:BaseEntityDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public UserRole Role { get; set; }
    public BranchOption Branch { get; set; }
}
public enum UserRole
{
    Admin,
    Secretary,
    User
}

