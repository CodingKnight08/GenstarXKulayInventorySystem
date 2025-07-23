

namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class RegistrationDto
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
}
public class LoginDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}