using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.SearchItems;

public partial class SearchItems
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<SearchItems> Logger { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    private IEnumerable<StocksDto> Results { get; set; } = new List<StocksDto>();
    private BranchOption Branch { get; set; } 
    private bool IsLoading { get; set; } = false;
    private string SearchItem { get; set; } = string.Empty;
    private bool IsBrand { get; set; }
    private bool ShowPrices = false;
    protected override async Task OnInitializedAsync()
    {
        Branch = UserState.Branch.Value;
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        IsLoading = true;

        try
        {
            var url = $"api/product/all/stocks/{Branch}?isBrand={IsBrand}&searchText={Uri.EscapeDataString(SearchItem)}";

            var response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            Results = await response.Content.ReadFromJsonAsync<List<StocksDto>>() ?? [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while loading products.");
            Results = [];
        }
        finally
        {
            IsLoading = false;
        }
    }


    private async Task Search()
    {
        
        await LoadProducts();
    }
   
    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            await Search();
        }
    }
    private string GetOtherBranchName()
    {
        return Branch switch
        {
            BranchOption.Polomolok => "General Santos City",
            BranchOption.GeneralSantosCity => "Polomolok",
            _ => "Other Branch"
        };
    }
}
