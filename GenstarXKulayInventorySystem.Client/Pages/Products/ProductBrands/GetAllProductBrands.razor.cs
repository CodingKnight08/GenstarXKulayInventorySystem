using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class GetAllProductBrands
{
    [Parameter] public int? Skip { get; set; }
    [Parameter] public int? Take { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected List<ProductBrandDto> ProductBrands { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected int PageSkip { get; set; }
    protected int PageTake { get; set; }
    private int Count { get; set; }
    private MudTable<ProductBrandDto>? brandsTable;


    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
     
    {
        PageSkip = Skip ?? 10;
        PageTake = Take ?? 10;

        await LoadProductBrandsAsync();
    }


    private async Task<TableData<ProductBrandDto>> LoadServerData(TableState state, CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {

            PageSkip = state.Page * state.PageSize;
            PageTake = state.PageSize;

            var response = await HttpClient.GetAsync($"api/productbrand/all?skip={PageSkip}&take={PageTake}");
            response.EnsureSuccessStatusCode();

            var brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>() ?? new();

            return new TableData<ProductBrandDto>
            {
                Items = brands,
                TotalItems = Count
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product brands: {ex.Message}");
            return new TableData<ProductBrandDto>
            {
                Items = new List<ProductBrandDto>(),
                TotalItems = 0
            };
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadProductBrandsAsync()
    {
        
        try
        {
            var response = await HttpClient.GetAsync("api/productbrand/all/count");

            response.EnsureSuccessStatusCode();

            Count = await response.Content.ReadFromJsonAsync<int>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product brands: {ex.Message}");
            ErrorMessage = "Failed to load product brands. Please try again later.";
        }

        
    }

    protected async Task CreateProductBrand()
    {
        var dialogRef = await DialogService.ShowAsync<CreateProductBrand>("Create Product Brand");

        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;

            if (result is not null && !result.Canceled && result.Data is ProductBrandDto)
            {
                await LoadProductBrandsAsync();
                if (brandsTable is not null)
            {
                await brandsTable.ReloadServerData();
            }
                StateHasChanged();
            }
        }
    }

    protected async Task UpdateProductBrand(int brandId)
    {
        var parameters = new DialogParameters
    {
        { "ProductBrandId", brandId }
    };
        var dialogRef = await DialogService.ShowAsync<EditProductBrand>("Update Product Brand",parameters);

        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;

            if (result is not null && !result.Canceled)
            {
                if (brandsTable is not null)
                {
                    await brandsTable.ReloadServerData();
                }
            }
        }
    }

    protected async Task DeleteProductBrand(int brandId)
    {
        var parameters = new DialogParameters
        {
            { "ProductBrandId", brandId }
        };
        var dialogRef = await DialogService.ShowAsync<DeleteProductBrand>("Delete Product Brand", parameters);
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                if (brandsTable is not null)
                {
                    await brandsTable.ReloadServerData();
                }
            }
        }
    }
    protected void ViewBrands(int brandId)
    {
        NavigationManager.NavigateTo($"/productbrand/{brandId}?skip={PageSkip}&take={PageTake}");

    }
}

