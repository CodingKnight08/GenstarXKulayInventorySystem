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
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<ViewDailySale> Logger { get; set; } = default!;

    protected DailySaleDto Sales { get; set; } = new DailySaleDto();
    protected DailySaleDto EditableSale { get; set; } = new DailySaleDto();
    protected bool IsLoading { get; set; } = false;
    protected bool IsEdit { get; set; } = false;
    protected bool IsSameDate => Sales.DateOfSales.Date == UtilitiesHelper.GetPhilippineTime().Date;
    protected override async Task OnInitializedAsync()
    {
        await LoadSale();
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
                TotalAmount = Sales.TotalAmount
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
}
