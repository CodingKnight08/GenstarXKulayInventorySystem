using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.OperationalProvider;

public partial class ViewOperationalProvider
{
    [Parameter] public int Id { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<ViewOperationalProvider> Logger { get; set; } = default!;
    protected bool IsLoading { get; set; } = true;
    protected OperationsProviderDto ProviderDto { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    protected async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/operationsprovider/{Id}");
            response.EnsureSuccessStatusCode();
            ProviderDto = await response.Content.ReadFromJsonAsync<OperationsProviderDto>() ?? new OperationsProviderDto();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading operational provider data.");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
