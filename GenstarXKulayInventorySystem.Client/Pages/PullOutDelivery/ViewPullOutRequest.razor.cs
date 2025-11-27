using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class ViewPullOutRequest
{
    [Parameter] public int PullOutId { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private ILogger<ViewPullOutRequest> Logger { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    private PullOutRequestDto PullOutRequest { get; set; } = new PullOutRequestDto();
    private bool IsLoading { get; set; } = false;

    private List<BreadcrumbItem> _items =
   [
     
        new("Request Items", href: "/requestitems"),
        new("View Pull Out", href: null, disabled: true)
   ];
    protected override async Task OnParametersSetAsync()
    {
        await LoadPullOut();
    }

    protected async Task LoadPullOut()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/pullout/{PullOutId}");
            if (response.IsSuccessStatusCode)
            {
                PullOutRequest = await response.Content.ReadFromJsonAsync<PullOutRequestDto>();
            }
            else
            {
                Snackbar.Add("Failed to load pull-out request.", Severity.Error);
                Logger.LogError("Error loading pull-out request with ID {PullOutId}. Status Code: {StatusCode}", PullOutId, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to load pull-out request.", Severity.Error);
            Logger.LogError(ex.Message, "Error loading pull-out request with ID {PullOutId}", PullOutId);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
