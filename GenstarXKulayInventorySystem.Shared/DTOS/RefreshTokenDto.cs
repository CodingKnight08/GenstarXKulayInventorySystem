namespace GenstarXKulayInventorySystem.Shared.DTOS;

public class RefreshTokenDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
}
