namespace GenstarXKulayInventorySystem.Server.Model;

public class RefreshToken
{
    public Guid Id { get; set; }

    public string Token { get; set; }

    public string UserId { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool Revoked { get; set; }

    public virtual User User { get; set; }
}
