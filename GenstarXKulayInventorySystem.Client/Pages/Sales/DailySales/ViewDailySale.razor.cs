using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.DailySales;

public partial class ViewDailySale
{
    [Parameter] public int Id { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<ViewDailySale> Logger { get; set; } = default!;

    protected DailySaleDto Sales { get; set; } = new DailySaleDto();
    protected DailySaleDto EditableSale { get; set; } = new DailySaleDto();
    protected bool IsLoading { get; set; } = false;
    protected bool IsEdit { get; set; } = false;
    protected bool IsSameDate => Sales.DateOfSales.Date == UtilitiesHelper.GetPhilippineTime().Date || Sales.PaymentType == null;
    protected override async Task OnInitializedAsync()
    {
        await LoadSale();
    }

    protected async Task LoadSaleItems()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/sales/items/{Sales.Id}");
            response.EnsureSuccessStatusCode();
            var saleItems = await response.Content.ReadFromJsonAsync<List<SaleItemDto>>();
            Sales.SaleItems = saleItems ?? new List<SaleItemDto>();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in loading Sale Items: {ex.Message}");
        }
    }
    protected async Task LoadSale()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/sales/{Id}");
            response.EnsureSuccessStatusCode();
            var sale = await response.Content.ReadFromJsonAsync<DailySaleDto>();
            Sales = sale ?? new DailySaleDto();
        }

        catch (Exception ex) {
            Logger.LogError($"Failed to load sales, {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

        protected List<BreadcrumbItem> _items =
       [
           new("Daily Sales", href: "/sales"),
            new("View Daily Sale", href: null, disabled: true)
       ];

    protected void UpdateSale() {
        if (!IsEdit)
        {
            EditableSale = new DailySaleDto
            {
                Id = Sales.Id,
                NameOfClient = Sales.NameOfClient,
                PaymentType = Sales.PaymentType,
                SalesOption = Sales.SalesOption,
                SalesNumber = Sales.SalesNumber,
                Branch = Sales.Branch,
                RecieptReference = Sales.RecieptReference,
                DateOfSales = Sales.DateOfSales,
                TotalAmount = Sales.TotalAmount,
                PaymentTermsOption = Sales.PaymentTermsOption,
                CreatedAt = Sales.CreatedAt,
                CustomPaymentTermsOption = Sales.CustomPaymentTermsOption,
                ExpectedPaymentDate = Sales.ExpectedPaymentDate,
                SaleItems = Sales.SaleItems

            };
        }
        IsEdit = !IsEdit;
    }
    protected void CancelEdit()
    {
        
        IsEdit = false;
        StateHasChanged();
    }

    protected void SaveEdit(DailySaleDto updatedSale)
    {
       
        Sales = updatedSale;
        IsEdit = false;
        StateHasChanged();
    }

    protected async Task DeleteSale()
    {
        try
        {
            var parameter = new DialogParameters
            {
                {"Sale",Sales }
            };
            var options = new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true,
                BackdropClick = false,
            };
            var dialog = await DialogService.ShowAsync<DeleteDailySale>("", parameter, options);
            if (dialog != null) { 
                var result = await dialog.Result;
                if (result != null && !result.Canceled) { 
                    IsLoading = true;
                    await Task.Delay(1000);
                    NavigationManager.NavigateTo("/sales");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occured: {ex.Message}");
        }
    }
}
