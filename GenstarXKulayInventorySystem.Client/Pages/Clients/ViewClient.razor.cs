using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class ViewClient
{
    [Parameter] public int Id { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewClient> Logger { get; set; } = default!;

    protected ClientDto Client { get; set; } = new ClientDto();

    protected bool IsLoading { get; set; } = false;
    protected List<BreadcrumbItem> _items =
   [
       new("Clients", href: "/clients"),
        new("View Client", href: null, disabled: true)
   ];
    protected override async Task OnInitializedAsync()
    {
        await LoadClient();
    }
    protected async Task LoadClient()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/client/{Id}");
            response.EnsureSuccessStatusCode();
            var client = await response.Content.ReadFromJsonAsync<ClientDto>();
            Client = client ?? new ClientDto();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading client with Id {Id}, with error {ex.Message}");
        }
        finally
        {
            await Task.Delay(1000);
            IsLoading = false;
        }
    }
}
