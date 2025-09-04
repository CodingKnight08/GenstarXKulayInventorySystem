using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Users;

public partial class UpdateUser
{
    [Parameter] public UserDto User { get; set; } = new();
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<UpdateUser> Logger { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;


    protected bool IsUpdating { get; set; } = false; 
    protected async Task UpdateUserAsync()
    {
        IsUpdating = true;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/authentication/{User.Id}", User);
            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("User updated successfully.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(User));
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                SnackBar.Add($"Error updating user: {errorMessage}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error updating user: {ex}");
            SnackBar.Add("An error occurred while updating the user.", Severity.Error);
        }
        IsUpdating = false;
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
