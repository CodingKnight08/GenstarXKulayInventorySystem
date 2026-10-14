using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Dashboard;

public partial class WeeklyTopSaleItems
{
    [Parameter] public BranchOption Branch { get; set; }
    [Parameter] public DateTime SelectedWeek { get; set; }
    [Inject]
    private HttpClient Http { get; set; } = default!;


    private bool IsLoading = true;


    private List<TopSaleItemDto> Products = new();



    protected override async Task OnParametersSetAsync()
    {
        await Task.Delay(500);
        await LoadProducts();
    }



    private async Task LoadProducts()
    {
        try
        {
            IsLoading = true;

            Products = await Http.GetFromJsonAsync<List<TopSaleItemDto>>(
                $"api/dashboard/weekly-top-products/{Branch}?date={SelectedWeek:yyyy-MM-dd}")
                ?? new();
        }
        finally
        {
            IsLoading = false;
        }
    }
}
