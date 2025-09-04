using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages;

public partial class RegistrationPage
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<RegistrationPage> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    protected RegistrationDto NewRegistrant { get; set; } = new RegistrationDto();
    private bool showPassword;
    private bool showConfirm;

    // ✅ Use the enum, not string
    private InputType passwordInputType => showPassword ? InputType.Text : InputType.Password;
    private InputType confirmInputType => showConfirm ? InputType.Text : InputType.Password;

    // MudBlazor Material icons are strings
    private string passwordIcon => showPassword ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility;
    private string confirmPasswordIcon => showConfirm ? Icons.Material.Filled.VisibilityOff : Icons.Material.Filled.Visibility;

    private void TogglePassword() => showPassword = !showPassword;
    private void ToggleConfirmPassword() => showConfirm = !showConfirm;
    protected MudForm? _form;
    private string _emailError { get; set; } = string.Empty;
    private string _passwordError { get; set; } = string.Empty;
    private string _userNameError { get; set; } = string.Empty;

    protected bool IsValid => !string.IsNullOrWhiteSpace(NewRegistrant.FullName) &&
    !string.IsNullOrWhiteSpace(NewRegistrant.Email) &&
    !string.IsNullOrWhiteSpace(NewRegistrant.Password) &&
    !string.IsNullOrWhiteSpace(NewRegistrant.ConfirmPassword);



    private bool ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(NewRegistrant.Email))
        {
            _emailError = "Email is required!";
            return true;
        }
        // Simple Regex for email format check
        if (!System.Text.RegularExpressions.Regex.IsMatch(
            NewRegistrant.Email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            _emailError = "Invalid email format!";
            return true;
        }

        return false; 
    }
    private bool ValidateUserName()
    {
        if (string.IsNullOrWhiteSpace(NewRegistrant.FullName))
        {
            _userNameError = "Full Name is required!";
            return false;
        }
        else
        {
            _userNameError = string.Empty;
            return true;
        }
    }
    private bool ValidatePassword()
    {
        if(string.IsNullOrWhiteSpace(NewRegistrant.Password) || string.IsNullOrWhiteSpace(NewRegistrant.ConfirmPassword))
        {
            _passwordError = "Password and Confirm Password are required!";
            return false;
        }
        else if(NewRegistrant.Password != NewRegistrant.ConfirmPassword)
        {
            _passwordError = "Password and Confirm Password do not match!";
            return false;
        }
        else if(NewRegistrant.Password.Length < 6 || NewRegistrant.ConfirmPassword.Length < 6)
        {
            _passwordError = "Password must be at least 6 characters long!";
            return false;
        }
        else{
            _passwordError = string.Empty;
            return true;
        }
    }

    protected async Task SubmitUser()
    {

        ValidateEmail();
        ValidatePassword();
        ValidateUserName();
        try
        {
            if (string.IsNullOrWhiteSpace(_emailError) && string.IsNullOrWhiteSpace(_passwordError))
            {
                var response = await HttpClient.PostAsJsonAsync("api/authentication/register", NewRegistrant);

                if (response.IsSuccessStatusCode)
                {
                    NavigationManager.NavigateTo("/");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Logger.LogError("Registration failed: {Error}", error);
                    Snackbar.Add("Registration failed. " + error, Severity.Error);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred during registration.");
        }
    }

 


}
