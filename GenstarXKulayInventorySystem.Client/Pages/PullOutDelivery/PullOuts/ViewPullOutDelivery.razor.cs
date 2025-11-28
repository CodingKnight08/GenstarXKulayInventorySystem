using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery.PullOuts;

public partial class ViewPullOutDelivery
{
    [Parameter] public int Id { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewPullOutDelivery> Logger { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected PullOutRequestDto PullOutRequest { get; set; } = new PullOutRequestDto();
    protected PullOutRequestDto EditablePullOut { get; set; } = new PullOutRequestDto();
    protected bool IsLoading { get; set; } = false;
    protected bool IsEdit { get; set; } = false;
    protected override async Task OnInitializedAsync()
    {
        await LoadPullOutRequest();
    }

    protected async Task LoadPullOutRequest()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/pullout/{Id}");
            if (response.IsSuccessStatusCode)
            {
                var pullOut = await response.Content.ReadFromJsonAsync<PullOutRequestDto>();
                if (pullOut != null)
                {
                    PullOutRequest = pullOut;
                    EditablePullOut = new PullOutRequestDto
                    {
                        Id = PullOutRequest.Id,
                        Status = PullOutRequest.Status,
                        Delivered = PullOutRequest.Delivered,
                        BranchRequestedTo = PullOutRequest.BranchRequestedTo,
                        BranchRequestee = PullOutRequest.BranchRequestee,
                        Note = PullOutRequest.Note,
                        RequestProductItems = PullOutRequest.RequestProductItems
                                    .Select(p => new RequestProductItemDto
                                    {
                                        Id = p.Id,
                                        PullOutRequestId = p.PullOutRequestId,
                                        ProductName = p.ProductName,
                                        RequestedQuantity = p.RequestedQuantity,
                                        ReleasedQuantity = p.ReleasedQuantity,
                                        IsReceived = p.IsReceived,
                                        Branch = p.Branch,
                                        Remarks = p.Remarks,
                                    }).ToList()
                    };
                }
            }
            else
            {
                Logger.LogError("Failed to load pull-out request. Status Code: {StatusCode}", response.StatusCode);
                Snackbar.Add("Failed to load pull-out request.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading pull-out request.");
            Snackbar.Add("An error occurred while loading the pull-out request.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task UpdatePullOut()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.PutAsJsonAsync("api/pullout", EditablePullOut);
            if (response.IsSuccessStatusCode)
            {
                var result = await HttpClient.PutAsJsonAsync("api/pullout/items", EditablePullOut.RequestProductItems);
                if (!result.IsSuccessStatusCode)
                {
                    Logger.LogError("Failed to update pull-out request items. Status Code: {StatusCode}", result.StatusCode);
                    Snackbar.Add("Failed to update pull-out request items.", Severity.Error);
                    return;
                }
                Snackbar.Add("Pull-out request updated successfully.", Severity.Success);
                await LoadPullOutRequest();
                IsEdit = false;
            }
            else
            {
                Logger.LogError("Failed to update pull-out request. Status Code: {StatusCode}", response.StatusCode);
                Snackbar.Add("Failed to update pull-out request.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating pull-out request.");
            Snackbar.Add("An error occurred while updating the pull-out request.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void CancelEdit()
    {
        IsEdit = false;
        EditablePullOut = new PullOutRequestDto
        {
            Id = PullOutRequest.Id,
            Status = PullOutRequest.Status,
            Note = PullOutRequest.Note,
            RequestProductItems = PullOutRequest.RequestProductItems
                        .Select(p => new RequestProductItemDto
                        {
                            Id = p.Id,
                            ProductName = p.ProductName,
                            RequestedQuantity = p.RequestedQuantity,
                            ReleasedQuantity = p.ReleasedQuantity,
                            IsReceived = p.IsReceived,
                            Branch = p.Branch
                        }).ToList()
        };
        StateHasChanged();
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
