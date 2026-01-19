using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Clients;

public partial class ViewClient
{
    [Parameter] public int Id { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "pageskip")]
    public int PageSkip { get; set; } = 0;
    [Parameter, SupplyParameterFromQuery(Name = "pagetake")]
    public int PageTake { get; set; } = 10;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewClient> Logger { get; set; } = default!;

    protected ClientDto Client { get; set; } = new ClientDto();

    protected bool IsLoading { get; set; } = false;
    protected List<BreadcrumbItem> _items { get; set; } = new List<BreadcrumbItem>();
    protected override async Task OnInitializedAsync()
    {
        await LoadClient();
    }
    protected override void OnParametersSet()
    {
        if (PageTake <= 0)
            PageTake = 10;

        if (PageSkip < 0)
            PageSkip = 0;
        _items =
           [
               new("Clients", href: $"/clients?pageskip={PageSkip}&pagetake={PageTake}"),
                    new("View Client", href: null, disabled: true)
           ];
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
