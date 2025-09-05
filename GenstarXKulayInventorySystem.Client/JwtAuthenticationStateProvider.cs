using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GenstarXKulayInventorySystem.Client;

public class JwtAuthenticationStateProvider: AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            // Not logged in
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(anonymous);
        }

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void NotifyUserLogout()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }
}
