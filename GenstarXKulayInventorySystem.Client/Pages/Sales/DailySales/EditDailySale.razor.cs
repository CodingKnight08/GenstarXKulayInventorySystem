using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class EditDailySale
{
    [Parameter] public DailySaleDto Sale { get; set; } = new DailySaleDto();
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback<DailySaleDto> OnSave { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<EditDailySale> Logger { get; set; } = default!;
    protected bool IsLoading { get; set; } = false;
    protected List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();

    protected override async Task OnInitializedAsync()
    {
        await LoadSaleItems();
    }

    protected async Task LoadSaleItems()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/salesitem/all/{Sale.Id}");
            response.EnsureSuccessStatusCode();
            var saleItems = await response.Content.ReadFromJsonAsync<List<SaleItemDto>>();
            SaleItems = saleItems ?? new List<SaleItemDto>();
        }
        catch (Exception ex) {
            Logger.LogError($"Error occured in loading Sale items, {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task HandleSave()
    {
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/sales/{Sale.Id}", Sale);

            if (response.IsSuccessStatusCode)
            {

                await OnSave.InvokeAsync(Sale);
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Logger.LogError($"Failed to save sale {Sale.Id}. Status: {response.StatusCode}, Error: {errorMessage}");
            }
        }
        catch (Exception ex) { 
        
            Logger.LogError($"Failed to save {ex}");
        }
        await OnSave.InvokeAsync(Sale);
    }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }

    private void HandleSaleItemsChanged(List<SaleItemDto> updatedItems)
    {
        SaleItems = updatedItems;
        Sale.SaleItems = updatedItems; 
    }
}

