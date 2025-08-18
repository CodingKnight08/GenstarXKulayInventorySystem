using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class CreateDailySale
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<CreateDailySale> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogInstance { get; set; } = default!;

    protected DailySaleDto Sale { get; set; } = new DailySaleDto();
    protected bool IsLoading { get; set; } = false;
    protected string? ErrorMessage { get; set; }
    protected bool IsValid => !string.IsNullOrWhiteSpace(Sale.NameOfClient) && !string.IsNullOrWhiteSpace(Sale.RecieptReference) && Sale.SalesOption != null && Sale.SaleItems.Count != 0;
    protected async Task CreateSaleAsync()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/sales",Sale);
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Sale created successfully!", Severity.Success);
                NavigationManager.NavigateTo("/sales");
            }
        }
        catch (Exception ex) {
            ErrorMessage = $"Failed to create sale with error, {ex.Message}";
            Logger.LogError("Error occured creating sale");
        }
        IsLoading = false;
    }

    protected void Cancel()
    {
        NavigationManager.NavigateTo("/sales");
    }
    private void HandleSaleItemsChanged(List<SaleItemDto> saleItems)
    {
        Sale.SaleItems = saleItems; // 🔗 Bind sale items to DailySaleDto
    }
}
