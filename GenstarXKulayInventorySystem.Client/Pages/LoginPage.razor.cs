using Blazored.LocalStorage;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace GenstarXKulayInventorySystem.Client.Pages;

public partial class LoginPage
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected ILogger<LoginPage> Logger { get; set; } = default!;
    [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] private JwtAuthenticationStateProvider JwtAuthProvider { get; set; } = default!;


    protected LoginDto User { get; set; } = new LoginDto();
    protected MudForm form = default!;
    protected bool ShowValidation { get; set; } = false;
    protected bool IsUsernameInvalid => string.IsNullOrWhiteSpace(User.Username);
    protected bool IsPasswordInvalid => string.IsNullOrWhiteSpace(User.Password);

    protected override async Task OnInitializedAsync()
    {
        var token = await LocalStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token) && JwtIsValid(token))
        {
            NavigationManager.NavigateTo("/dashboard", forceLoad: true);
        }
        else
        {
            await LocalStorage.RemoveItemAsync("authToken");
        }
    }

    private async Task Login()
    {
        ShowValidation = true;

        if (IsUsernameInvalid || IsPasswordInvalid)
            return;

        try
        {
            var response = await Http.PostAsJsonAsync("api/authentication/login", User);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                await LocalStorage.RemoveItemAsync("authToken");
                SnackBar.Add("Invalid username or password.", Severity.Error);
                Logger.LogWarning("Login failed: {Error}", error);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Token is null)
            {
                await LocalStorage.RemoveItemAsync("authToken");
                SnackBar.Add("Login failed: no token returned.", Severity.Error);
                return;
            }

            var cleanToken = result.Token.Trim().Trim('"');

            // Decode JWT to inspect claims
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(cleanToken);

            foreach (var claim in jwt.Claims)
                Logger.LogInformation("JWT Claim: {Type} = {Value}", claim.Type, claim.Value);

            // Save token in local storage
            await LocalStorage.SetItemAsync("authToken", cleanToken);

            // Notify AuthenticationStateProvider
            JwtAuthProvider.NotifyUserAuthentication(cleanToken);

            NavigationManager.NavigateTo("/dashboard", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred during login.");
            SnackBar.Add("An unexpected error occurred. Please try again.", Severity.Error);
        }
    }

    protected void Register()
    {
        NavigationManager.NavigateTo("/register");
    }

    private bool JwtIsValid(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

}

