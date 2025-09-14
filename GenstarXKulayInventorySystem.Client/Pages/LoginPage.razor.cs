using Blazored.LocalStorage;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.WebRequestMethods;
using System.IdentityModel.Tokens.Jwt;

namespace GenstarXKulayInventorySystem.Client.Pages;

public partial class LoginPage
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected ILogger<LoginPage> Logger { get; set; } = default!;
    [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;


    protected LoginDto User { get; set; } = new LoginDto();
    protected MudForm form = default!;
    protected bool ShowValidation { get; set; } = false;
    protected bool IsUsernameInvalid => string.IsNullOrWhiteSpace(User.Username);
    protected bool IsPasswordInvalid => string.IsNullOrWhiteSpace(User.Password);

    protected override async Task OnInitializedAsync()
    {
        var token = await LocalStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrWhiteSpace(token))
        {
            if (JwtIsValid(token))
            {
                NavigationManager.NavigateTo("/dashboard", forceLoad: true);
            }
            else
            {
                await LocalStorage.RemoveItemAsync("authToken"); 
            }
        }
    }


    private async Task Login()
    {
        ShowValidation = true;

        if (string.IsNullOrWhiteSpace(User?.Username) || string.IsNullOrWhiteSpace(User?.Password))
            return;

        try
        {
            var response = await Http.PostAsJsonAsync("api/authentication/login", User);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                // Invalid login → redirect home and show error
                await LocalStorage.RemoveItemAsync("authToken");

                SnackBar.Add("Invalid username or password.", Severity.Error);
                Logger.LogWarning("Login failed: {Error}", error);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is not null && !string.IsNullOrEmpty(result.Token))
            {
                await LocalStorage.SetItemAsync("authToken", result.Token);
                NavigationManager.NavigateTo("/dashboard", forceLoad: true);
            }
            else
            {
                // No token returned from backend → treat as failed login
                await LocalStorage.RemoveItemAsync("authToken");
                NavigationManager.NavigateTo("/", forceLoad: true);

                SnackBar.Add("Login failed: no token returned.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while attempting to log in.");
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

            // JWT expiration is in UTC
            return jwt.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false; // token was invalid or corrupt
        }
    }

}

