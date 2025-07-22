namespace GenstarXKulayInventorySystem.Server.Model;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
}


public enum UserRole
{
    Admin,
    Secretary,
    User
}