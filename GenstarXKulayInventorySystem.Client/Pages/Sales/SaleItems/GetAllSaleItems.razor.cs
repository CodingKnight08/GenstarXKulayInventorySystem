using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Sales.SaleItems;

public partial class GetAllSaleItems
{

    [Parameter] public int DailySaleId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllSaleItems> Logger { get; set; } = default!;
    protected bool IsLoading { get; set; } = false;
    protected List<SaleItemDto> SaleItems { get; set; } = new List<SaleItemDto>();

    protected override async Task OnInitializedAsync()
    {
        await LoadSaleItems();
    }

    private async Task LoadSaleItems()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/salesitem/all/{DailySaleId}");
            response.EnsureSuccessStatusCode();
            var saleItems = await response.Content.ReadFromJsonAsync<List<SaleItemDto>>();
            SaleItems = saleItems ?? new List<SaleItemDto>();
        }
        catch (Exception ex) {
                Logger.LogError($"Error in loading Sale Items: {ex.Message}");
        }
        finally
        {
            await Task.Delay(2000);
            IsLoading = false;
        }
    }

    protected async Task ViewSaleItemAsync(int id)
    {
        var parameter = new DialogParameters
        {
            {"SaleItemId",id }
        };
        var options = new DialogOptions
        {
            CloseOnEscapeKey = false,
            BackdropClick = false,
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
        };

        var dialog = await DialogService.ShowAsync<ViewSaleItem>("Sale Item Info", parameter, options);
       
    }
}
