

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class UserDto:BaseEntityDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }
}
public enum UserRole
{
    Admin,
    Secretary,
    User
}

