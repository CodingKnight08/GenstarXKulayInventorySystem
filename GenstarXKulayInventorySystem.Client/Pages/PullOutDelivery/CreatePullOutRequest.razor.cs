using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.PullOutDelivery;

public partial class CreatePullOutRequest
{
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<CreatePullOutRequest> Logger { get; set; } = default!;

    protected PullOutRequestDto NewPullOut { get; set; } = new();
    protected List<RequestProductItemDto> RequestedItems { get; set; } = new();
    protected bool IsSubmitting = false;
    protected BranchOption Requester { get; set; }
    protected bool CanSubmit => RequestedItems.Count > 0 && !IsSubmitting;
    protected override void OnInitialized()
    {
        NewPullOut.DateRequest = DateTime.Now;
        NewPullOut.BranchRequestee = UserState.Branch.GetValueOrDefault();
        Requester = UserState.Branch.GetValueOrDefault();
    }

    private void OnBranchChanged(BranchOption newBranch)
    {
        NewPullOut.BranchRequestedTo = newBranch;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task SubmitRequest()
    {
        if (RequestedItems.Count == 0)
        {
            Snackbar.Add("You must add at least 1 product to request.", Severity.Warning);
            return;
        }

        try
        {
            IsSubmitting = true;

            // Ensure PullOutRequest reference is null to prevent model validation errors
            foreach (var item in RequestedItems)
            {
                item.PullOutRequest = null;
            }

            NewPullOut.RequestProductItems = RequestedItems;

            // POST to API
            var response = await HttpClient.PostAsJsonAsync("api/pullout", NewPullOut);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Pull-out request created successfully!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                var msg = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed: {msg}", Severity.Error);
            }
        }
        catch (HttpRequestException httpEx)
        {
            Snackbar.Add($"Request error: {httpEx.Message}", Severity.Error);
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Unexpected error: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsSubmitting = false;
        }
    }



    public async Task AddItem()
    {
        var parameters = new DialogParameters
    {
        { "BranchSource", NewPullOut.BranchRequestedTo }
    };

        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            BackdropClick = false
        };

        var dialog = await DialogService.ShowAsync<RequestProducts>("Add Product Item", parameters, options);

        var result = await dialog.Result;

        if (!result.Canceled && result.Data is RequestProductItemDto item)
        {
            // Check duplicate by MasterId
            var existing = RequestedItems.FirstOrDefault(i => i.MasterProductId == item.MasterProductId);

            if (existing != null)
            {
                // Merge quantity
                existing.RequestedQuantity += item.RequestedQuantity;
            }
            else
            {
                RequestedItems.Add(item);
            }

            Snackbar.Add("Item added.", Severity.Success);
        }
        StateHasChanged();
    }
    private async Task DeleteItem(RequestProductItemDto item)
    {
        bool? confirm = await DialogService.ShowMessageBox(
            "Confirm Delete",
            $"Are you sure you want to remove '{item.ProductName}'?",
            yesText: "Yes, remove",
            cancelText: "Cancel"
        );

        if (confirm == true)
        {
            RequestedItems.Remove(item);
            Snackbar.Add("Item removed.", Severity.Info);
            StateHasChanged();
        }
    }
}
