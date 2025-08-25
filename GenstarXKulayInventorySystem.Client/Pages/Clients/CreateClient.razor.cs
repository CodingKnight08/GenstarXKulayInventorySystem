using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class CreateClient
{
    [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected bool IsValid => !string.IsNullOrWhiteSpace(Client.ClientName);
    protected ClientDto Client { get; set; } = new ClientDto();

    protected async Task SubmitClient()
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/client", Client);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Client added successfully!", Severity.Success);
                Dialog.Close(DialogResult.Ok(Client));
            }
        }
        catch (Exception ex) {
            Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
        }
       
    }
    private void Cancel() => Dialog.Cancel();
}
