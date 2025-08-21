using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems;

public partial class ViewSaleItem
{
    [Parameter] public int SaleItemId { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<ViewSaleItem> Logger { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance Dialog { get; set; } = default!;
    protected SaleItemDto SaleItem { get; set; } = new SaleItemDto();
    protected List<InvolvePaintsDto> Paints { get; set; } = new List<InvolvePaintsDto>();
    protected bool IsLoading { get; set; } = false;
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadSaleItem();
        LoadPaintItems();
    }

    protected async Task LoadSaleItem()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/salesitem/{SaleItemId}");
            response.EnsureSuccessStatusCode();
            var sale = await response.Content.ReadFromJsonAsync<SaleItemDto>();
            SaleItem = sale ?? new SaleItemDto();   
        }
        catch (Exception ex) {
            ErrorMessage = "An unexpected error occurred while loading the sale item.";
            Logger.LogError(ex, "Exception while fetching SaleItem {SaleItemId}", SaleItemId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void LoadPaintItems()
    {
        if(SaleItem != null)
        {
            Paints = SaleItem.DataList.ToList();
        }
    }

    private void Cancel() => Dialog.Close(); 
}
