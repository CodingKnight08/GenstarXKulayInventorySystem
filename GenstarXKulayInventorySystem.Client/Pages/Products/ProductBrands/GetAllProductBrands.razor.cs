using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class GetAllProductBrands
{
  
    [Inject] protected ILogger<GetAllProductBrands> Logger { get; set; } = default!;
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected List<ProductBrandDto> ProductBrands { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    private MudDataGrid<ProductBrandDto>? brandsGrid;
    private int Count { get; set; }

    protected string? ErrorMessage { get; set; }
    private int PageSkip { get; set; } = 0;
    private int PageTake { get; set; } = 10;
    private int CurrentPage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {

            await LoadProductBrandsAsync(); // load Count if needed
        }
        catch (Exception ex)
        {
            Logger.LogError($"An error occured upon initialization: {ex.Message}", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }


    protected void OnPageChanged(int page)
    {
        CurrentPage = page;
        PageSkip = CurrentPage * PageTake;
        StateHasChanged();
    }
    protected void OnRowsPerPageChanged(int newPageSize)
    {
        PageTake = newPageSize;
        CurrentPage = 0; 
        PageSkip = 0;
    }

    private async Task<GridData<ProductBrandDto>> ServerLoadData(GridState<ProductBrandDto> state)
    {
        try
        {
            // figure out skip/take from grid state
            var skip = state.Page * state.PageSize;
            var take = state.PageSize;

            // fetch paged items
            var response = await HttpClient.GetAsync($"api/productbrand/all?skip={skip}&take={take}");
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>() ?? new();



            return new GridData<ProductBrandDto>
            {
                Items = items,
                TotalItems = Count
            };
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading product brands: {ex.Message}");
            return new GridData<ProductBrandDto>
            {
                Items = new List<ProductBrandDto>(),
                TotalItems = 0
            };
        }
    }





    private async Task LoadProductBrandsAsync()
    {
        IsLoading = true;
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
        IsLoading = false;
        
    }

    
    protected async Task CreateProductBrand()
    {
        var dialogOptions = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            BackdropClick = false
        };
        var dialogRef = await DialogService.ShowAsync<CreateProductBrand>("Create Product Brand",dialogOptions);

        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;

            if (result is not null && !result.Canceled && result.Data is ProductBrandDto)
            {
                await LoadProductBrandsAsync();
                await ReloadGridAsync();
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
                await ReloadGridAsync();
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
                await LoadProductBrandsAsync();
                await ReloadGridAsync();
                StateHasChanged();
            }
        }
    }
    protected void ViewBrands(int brandId)
    {
        NavigationManager.NavigateTo($"/productbrand/{brandId}");
    }


    private async Task ReloadGridAsync()
    {
        if (brandsGrid is not null)
        {
            await brandsGrid.ReloadServerData();
        }
    }


}

