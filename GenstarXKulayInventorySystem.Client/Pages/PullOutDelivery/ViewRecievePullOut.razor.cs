using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class ViewRecievePullOut
{
    [Parameter] public int PullOutId { get; set; }
    [Parameter, SupplyParameterFromQuery] public int PageSkip { get; set; } = 0;
    [Parameter, SupplyParameterFromQuery] public int PageTake { get; set; } = 5;

    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private ILogger<ViewPullOutRequest> Logger { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    private PullOutRequestDto PullOutRequest { get; set; } = new PullOutRequestDto();
    private bool IsLoading { get; set; } = false;
    private bool IsEdit { get; set; } = false;
    private List<BreadcrumbItem> _items = new();
    protected override async Task OnParametersSetAsync()
    {
        _items =
   [
       new("Recieved Items", href: $"/receiveitems?pageskip={PageSkip}&pagetake={PageTake}"),
        new("View Pull Out", href: null, disabled: true)
   ];
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
                PullOutRequest = await response.Content.ReadFromJsonAsync<PullOutRequestDto>() ?? new PullOutRequestDto();
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
   

 

    Color MapColor(string key) => key switch
    {
        "warning" => Color.Warning,
        "info" => Color.Info,
        "success" => Color.Success,
        "error" => Color.Error,
        _ => Color.Default
    };
}
