using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace GenstarXKulayInventorySystem.Client.Pages;

public partial class LoginPage
{
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] protected ILogger<LoginPage> Logger { get; set; } = default!;


    protected LoginDto User { get; set; } = new LoginDto();
    protected MudForm form = default!;
    protected bool ShowValidation { get; set; } = false;
    protected bool IsUsernameInvalid => string.IsNullOrWhiteSpace(User.Username);
    protected bool IsPasswordInvalid => string.IsNullOrWhiteSpace(User.Password);
    protected override async Task OnInitializedAsync()
    {
     
        await Task.CompletedTask;
    }
    protected private async Task Login()
    {
        ShowValidation = true;

        if (string.IsNullOrWhiteSpace(User.Username) || string.IsNullOrWhiteSpace(User.Password))
            return;

        try
        {
            var response = await Http.PostAsJsonAsync("api/authentication/login", User);



            if (response.IsSuccessStatusCode)
            {
                // Success: Redirect
                NavigationManager.NavigateTo("/dashboard");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Logger.LogError("Login failed: {Error}", error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while attempting to log in.");
        }
    }





}

