using Blazored.LocalStorage;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly UserState _userState;
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage, UserState userState)
    {
        _localStorage = localStorage;
        _userState = userState;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        JwtSecurityToken jwt;

        try
        {
            jwt = _tokenHandler.ReadJwtToken(token);
        }
        catch
        {
            await _localStorage.RemoveItemAsync("authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            await _localStorage.RemoveItemAsync("authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = jwt.Claims.Select(c =>
            (c.Type == "role" || c.Type == ClaimTypes.Role)
                ? new Claim(ClaimTypes.Role, c.Value)
                : c
        ).ToList();

        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(claims, "jwtAuth", ClaimTypes.Name, ClaimTypes.Role)));
    }

    public void NotifyUserAuthentication(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            NotifyUserLogout();
            return;
        }

        var jwt = _tokenHandler.ReadJwtToken(token);

        var claims = jwt.Claims.Select(c =>
            (c.Type == "role" || c.Type == ClaimTypes.Role)
                ? new Claim(ClaimTypes.Role, c.Value)
                : c
        ).ToList();

        var identity = new ClaimsIdentity(claims, "jwtAuth", ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
}
