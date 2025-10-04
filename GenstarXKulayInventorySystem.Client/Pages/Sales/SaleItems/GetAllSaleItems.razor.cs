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
    protected MudTable<SaleItemDto>? saleItemsTable;
    protected override async Task OnInitializedAsync()
    {
        await LoadSaleItems();
    }

    private async Task LoadSaleItems()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/salesitem/all/{DailySaleId}?skip=0&take=10");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SaleItemPageResultDto<SaleItemDto>>();

            if (result != null)
            {
                SaleItems = result.SaleItems;
                
            }
            else
            {
                SaleItems = new List<SaleItemDto>();
                
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error in loading Sale Items: {ex.Message}");
        }
        finally
        {
            await Task.Delay(2000);
            IsLoading = false;
        }
    }



    private async Task<TableData<SaleItemDto>> ServerLoadData(TableState state, CancellationToken cancellationToken)
    {
        try
        {
            var skip = state.Page * state.PageSize;
            var take = state.PageSize;
            var response = await HttpClient.GetAsync($"api/salesitem/all/{DailySaleId}?skip={skip}&take={take}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SaleItemPageResultDto<SaleItemDto>>(cancellationToken:cancellationToken);
            if (result == null) {
                return new TableData<SaleItemDto> { Items = new List<SaleItemDto>(), TotalItems = 0 };
            }
            return new TableData<SaleItemDto>
            {
                Items = result.SaleItems,
                TotalItems = result.TotalCount,
            };

        }
        catch(Exception ex)
        {
            Logger.LogError($"Error loading sale items: {ex.Message}");
            return new TableData<SaleItemDto> { Items = new List<SaleItemDto>(), TotalItems = 0 };
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
