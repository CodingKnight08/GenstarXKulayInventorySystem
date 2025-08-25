using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class DeleteClient
{
    [Parameter] public int ClientId { get; set; }
    [CascadingParameter] protected IMudDialogInstance DialogInstance { get; set; } = default!;
    [Inject] protected ILogger<DeleteClient> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    
    protected bool IsLoading { get; set; } = false;
    protected ClientDto Client { get; set; } = new ClientDto();

    protected override async Task OnInitializedAsync()
    {
        await LoadClient();
    }
    protected async Task LoadClient()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/client/{ClientId}");
            response.EnsureSuccessStatusCode();
            var client = await response.Content.ReadFromJsonAsync<ClientDto>();
            if (client != null)
            {
                Client = client;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading client with id {ClientId}, with error {ex.Message}");
            SnackBar.Add("Failed to load client", Severity.Error);
            DialogInstance.Close();
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }

    protected async Task DeleteClientAsync()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/client/{ClientId}");
            response.EnsureSuccessStatusCode();
            SnackBar.Add("Client deleted successfully", Severity.Success);
            DialogInstance.Close(DialogResult.Ok(true));
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error deleting client with id {ClientId}, with error {ex.Message}");
            SnackBar.Add("Failed to delete client", Severity.Error);
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }

    protected void Cancel()
    {
        DialogInstance.Cancel();
    }

}
