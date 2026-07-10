using Blazored.LocalStorage;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;
namespace GenstarXKulayInventorySystem.Client.Pages;

public partial class LoginPage
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<LoginPage> Logger { get; set; } = default!;
    [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] private JwtAuthenticationStateProvider JwtAuthProvider { get; set; } =  default!;
    [Inject] public HttpClient Http { get; set; } = default!;
    protected LoginDto User { get; set; } = new LoginDto();
    protected MudForm form = default!;
    protected bool ShowValidation { get; set; } = false;
    protected bool ShowPassword { get; set; } = false;
    protected bool IsUsernameInvalid => string.IsNullOrWhiteSpace(User.Username);
    protected bool IsPasswordInvalid => string.IsNullOrWhiteSpace(User.Password);
    protected string PasswordInputType => ShowPassword ? "text" : "password";

    protected override async Task OnInitializedAsync()
    {
        var token = await LocalStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token) && JwtIsValid(token))
        {
            NavigationManager.NavigateTo("/dashboard", true);
        }
        else
        {
            await LocalStorage.RemoveItemAsync("authToken");
            await LocalStorage.RemoveItemAsync("refreshToken");
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
                await LocalStorage.RemoveItemAsync("authToken");
                await LocalStorage.RemoveItemAsync("refreshToken");

                SnackBar.Add("Invalid username or password.", Severity.Error);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (result == null ||
                string.IsNullOrWhiteSpace(result.AccessToken) ||
                string.IsNullOrWhiteSpace(result.RefreshToken))
            {
                await LocalStorage.RemoveItemAsync("authToken");
                await LocalStorage.RemoveItemAsync("refreshToken");

                SnackBar.Add("Login failed.", Severity.Error);
                return;
            }

            var cleanAccessToken = result.AccessToken.Trim().Trim('"');

            // Optional: Inspect JWT claims
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(cleanAccessToken);

            foreach (var claim in jwt.Claims)
            {
                Logger.LogInformation("JWT Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            // Save BOTH tokens
            await LocalStorage.SetItemAsync("authToken", cleanAccessToken);
            await LocalStorage.SetItemAsync("refreshToken", result.RefreshToken);

            // Notify AuthenticationStateProvider
            JwtAuthProvider.NotifyUserAuthentication(cleanAccessToken);

            SnackBar.Add("Login successful.", Severity.Success);

            NavigationManager.NavigateTo("/dashboard", true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Login failed.");

            await LocalStorage.RemoveItemAsync("authToken");
            await LocalStorage.RemoveItemAsync("refreshToken");

            SnackBar.Add("An unexpected error occurred.", Severity.Error);
        }
    }

    protected void Register()
    {
        NavigationManager.NavigateTo($"/register?client=false");
    }
    private void TogglePasswordVisibility()
    {
        ShowPassword = !ShowPassword;
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            _ = Login();
        }
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

