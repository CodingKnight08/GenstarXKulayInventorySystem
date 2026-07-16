using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.ReturnSales;

public partial class GetAllReturnSales
{
    [Parameter] public int SalesId { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "pageskip")]
    public int PageSkip { get; set; } = 0;

    [Parameter, SupplyParameterFromQuery(Name = "pagetake")]
    public int PageTake { get; set; } = 10;

    [Parameter, SupplyParameterFromQuery(Name = "date")]
    public string? DateString { get; set; }
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<GetAllReturnSales> Logger { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected List<ReturnItemDto> ReturnItems { get; set; } = new List<ReturnItemDto>();
    protected AddReturnSalesRequest AddReturnSalesRequest { get; set; } = new AddReturnSalesRequest();
    protected DateTime SelectedDate { get; set; }
    protected bool CanSaveReturns => ReturnItems.Any();
    protected bool IsSaving { get; set; } = false;
    protected override void OnParametersSet()
    {
        if (!string.IsNullOrWhiteSpace(DateString) &&
            DateTime.TryParse(DateString, out var parsedDate))
        {
            SelectedDate = parsedDate;
        }
       
    }


    protected async Task AddReturnItem()
    {
        var dialogParameters = new DialogParameters
    {
        { "SalesId", SalesId }
    };

        var dialogOptions = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            BackdropClick = true
        };

        var dialog = await DialogService.ShowAsync<AddReturnSales>(
            "Add Return Item",
            dialogParameters,
            dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false } &&
            result.Data is ReturnItemDto newItem)
        {
            if (IsDuplicate(newItem))
            {
                Snackbar.Add(
                    "This product is already added to the return list.",
                    Severity.Warning);
                return;
            }

            ReturnItems.Add(newItem);
            Snackbar.Add("Return item added to list.", Severity.Success);
            StateHasChanged();
        }
    }


    private bool IsDuplicate(ReturnItemDto newItem)
    {
        return ReturnItems.Any(x =>
            x.BranchProductId == newItem.BranchProductId);
    }
    protected void RemoveReturnItem(ReturnItemDto item)
    {
        if (item == null)
            return;

        ReturnItems.Remove(item);

        Snackbar.Add("Return item removed.", Severity.Info);
        StateHasChanged();
    }


    protected async Task SaveReturnItems()
    {
        if (IsSaving)
            return;

        IsSaving = true;

        try
        {
            // 1️⃣ Prepare request
            AddReturnSalesRequest.DailySaleId = SalesId;
            AddReturnSalesRequest.ReturnItems = ReturnItems;

            // 2️⃣ Save return items
            var saveResponse = await HttpClient.PostAsJsonAsync(
                "api/sales/returnitems",
                AddReturnSalesRequest);

            saveResponse.EnsureSuccessStatusCode();

            // 3️⃣ Calculate return total
            var returnTotal = ReturnItems.Sum(x => x.TotalPrice);

            // 4️⃣ Update sale total (DECIMAL ONLY)
            var updateResponse = await HttpClient.PutAsync(
                $"api/sales/return/{SalesId}?returnTotal={returnTotal}",
                content: null);

            updateResponse.EnsureSuccessStatusCode();

            Snackbar.Add("Return items saved successfully.", Severity.Success);

            NavigationManager.NavigateTo(
                $"/sales/view/{SalesId}?pageskip={PageSkip}&pagetake={PageTake}&date={SelectedDate:yyyy-MM-dd}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving return items");
            Snackbar.Add("An error occurred while saving return items.", Severity.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }


    protected void Cancel()
    {
        NavigationManager.NavigateTo($"/sales/view/{SalesId}?pageskip={PageSkip}&pagetake={PageTake}&date={SelectedDate:yyyy-MM-dd}");

    }
}
