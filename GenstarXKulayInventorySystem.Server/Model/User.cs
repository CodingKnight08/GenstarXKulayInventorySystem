using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Identity;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Model;

public class User: IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string CreatedBy { get; set; } = "Admin"; 
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public BranchOption  Branch { get;set; }
    public bool IsClient { get; set; } = false;
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}


