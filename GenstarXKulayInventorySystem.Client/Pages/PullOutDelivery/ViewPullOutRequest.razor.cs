using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class ViewPullOutRequest
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
       new("Request Items", href: $"/requestitems?pageskip={PageSkip}&pagetake={PageTake}"),
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
    private async Task OnStatusChanged(bool value, RequestProductItemDto context)
    {
        context.IsReceived = value;
        await UpdateStatus(context);
    }
    private async Task OnPullOutStatusChange(DeliveryStatusOption newStatus)
    {
        PullOutRequest.Status = newStatus;
        if(PullOutRequest.Status == DeliveryStatusOption.Delivered)
        {
            PullOutRequest.Delivered = true;
            PullOutRequest.DateDelivered = PhilippineTime.Now;
        }
        // example: delivered = false?
        await UpdatePullOutStatus(false);
    }

    protected async Task UpdatePullOutStatus(bool itemStatusChange)
    {
        try
        {
            var response = await HttpClient.PutAsJsonAsync("api/pullout", PullOutRequest);
            if(response.IsSuccessStatusCode)
            {
                if (!itemStatusChange)
                {
                    var toUpdateItems = PullOutRequest.RequestProductItems.Where(e => !e.IsReceived).ToList();
                    if(toUpdateItems.Count > 0)
                    {
                        foreach (var item in toUpdateItems)
                        {
                            item.IsReceived = true;
                        }
                        var result = await HttpClient.PutAsJsonAsync("api/pullout/items", toUpdateItems);
                        if (!result.IsSuccessStatusCode)
                        {
                            Snackbar.Add("Failed to update item statuses.", Severity.Error);
                            Logger.LogError("Error updating item statuses for PullOutRequest ID {PullOutId}. Status Code: {StatusCode}", PullOutRequest.Id, result.StatusCode);
                        }
                    }
                    
                    await LoadPullOut();
                }
                
                Snackbar.Add("Pull-out request updated successfully.", Severity.Success);
            }
            StateHasChanged();
            
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error updating pull-out request.");
            Snackbar.Add("An error occurred while updating the pull-out request.", Severity.Error);
        }
    }
    protected async Task UpdateStatus(RequestProductItemDto toBeUpdated)
    {
        try
        {
            if (PullOutRequest.Status != DeliveryStatusOption.Delivered && toBeUpdated.IsReceived)
            {
                PullOutRequest.Delivered = true;
                PullOutRequest.Status = DeliveryStatusOption.Delivered;
                PullOutRequest.DateDelivered = PhilippineTime.Now;
                await UpdatePullOutStatus(true);
            }
            var response = await HttpClient.PutAsJsonAsync("api/pullout/status", toBeUpdated);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Item status updated successfully.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Failed to update item status.", Severity.Error);
                Logger.LogError("Error updating item status for RequestProductItem ID {ItemId}. Status Code: {StatusCode}", toBeUpdated.Id, response.StatusCode);
            }
            await LoadPullOut();
            StateHasChanged();
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error updating item status for RequestProductItem ID {ItemId}", toBeUpdated.Id);
        }
    }
    private void ToggleEdit()
    {
        IsEdit = !IsEdit;
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
