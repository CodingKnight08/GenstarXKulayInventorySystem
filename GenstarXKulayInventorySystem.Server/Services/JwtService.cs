using GenstarXKulayInventorySystem.Server.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace GenstarXKulayInventorySystem.Server.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName
                ?? throw new InvalidOperationException("UserName is required")),
            new Claim(ClaimTypes.NameIdentifier, user.Id
                ?? throw new InvalidOperationException("User Id is required")),
            new Claim("Branch", user.Branch.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var jwtKey = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is missing from configuration.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
