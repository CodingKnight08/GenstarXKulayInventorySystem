using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class UpdateClient
{
    [Parameter] public int ClientId { get; set; }
    [CascadingParameter] protected IMudDialogInstance DialogInstance { get; set; } = default!;
    [Inject] protected ILogger<UpdateClient> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;

    protected ClientDto Client { get; set; } = new ClientDto();

    protected bool IsLoading { get; set; } = false; 
    protected bool IsValid => !string.IsNullOrWhiteSpace(Client.ClientName);
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

    protected async Task UpdateClientAsync()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/client/{ClientId}", Client);
            response.EnsureSuccessStatusCode();
            SnackBar.Add("Client updated successfully", Severity.Success);
            DialogInstance.Close(DialogResult.Ok(Client));
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error updating client with id {ClientId}, with error {ex.Message}");
            SnackBar.Add("Failed to update client", Severity.Error);
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
