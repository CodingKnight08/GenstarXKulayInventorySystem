using GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.Product;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class ViewBrand
{
    [Parameter] public int BrandId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] protected ILogger<ViewBrand> Logger { get; set; } = default!;
    protected ProductBrandDto Brand { get; set; } = new ProductBrandDto();
    protected List<BranchProductDto> Products { get; set; } = new List<BranchProductDto>();
    protected List<ProductCategoryDto> Categories { get; set; } = new List<ProductCategoryDto>();
    protected BranchOption Branch { get; set; }
    protected MudTable<BranchProductDto>? productTable;
    protected bool IsLoading = false;
    protected string? ErrorMessage { get; set; }
    protected List<BreadcrumbItem> items = new();
    protected int PageSkip { get; set; } = 0;
    protected int PageTake { get; set; } = 10;
    protected int CurrentPage { get; set; }
    protected int Count { get; set; }
    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        Branch = UserState.Branch ?? BranchOption.Warehouse;
        await LoadBrandAsync();
        //await LoadProductsAsync();
        await LoadCategoriesAsync();
        await LoadBranchProducts();
        items =
        [
            new("Brands", href: $"/productbrands"),
            new("Brand Detail", href: "#", disabled: true),
        ];
        
        await Task.Delay(1000);

        IsLoading = false;
    }
    


    protected async Task LoadBrandAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/productbrand/{BrandId}");
            response.EnsureSuccessStatusCode();
            Brand = await response.Content.ReadFromJsonAsync<ProductBrandDto>() ?? new ProductBrandDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching brand: {ex.Message}");
            ErrorMessage = "Failed to load brand details. Please try again later.";
        }
    }

    protected async Task LoadProductsAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/product/count/{BrandId}/{Branch}");
            response.EnsureSuccessStatusCode();
            Count = await response.Content.ReadFromJsonAsync<int>();

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching products: {ex.Message}");
            ErrorMessage = "Failed to load products. Please try again later.";
        }
    }

    protected async Task<TableData<BranchProductDto>> ServerLoadData(TableState state, CancellationToken cancellationToken)
    {
        try
        {
            var skip = state.Page * state.PageSize;
            var take = state.PageSize;

            var url = $"api/product/paged/by/{BrandId}/{Branch}?skip={skip}&take={take}";

            Logger.LogInformation($"Loading products → {url}");

            var response = await HttpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var serverMsg = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError($"Server returned {response.StatusCode}: {serverMsg}");

                return new TableData<BranchProductDto>
                {
                    Items = new List<BranchProductDto>(),
                    TotalItems = 0
                };
            }

            var result = await response.Content
                .ReadFromJsonAsync<BranchProductPageResultDto<BranchProductDto>>(cancellationToken);

            return new TableData<BranchProductDto>
            {
                Items = result?.Products ?? new List<BranchProductDto>(),
                TotalItems = result?.TotalCount ?? 0
            };
        }
        catch (Exception ex)
        {
            Logger.LogError($"Client Error loading products: {ex.Message}");
            return new TableData<BranchProductDto>
            {
                Items = new List<BranchProductDto>(),
                TotalItems = 0
            };
        }
    }
    protected async Task LoadBranchProducts()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/existing/products/{BrandId}/{Branch}");
            response.EnsureSuccessStatusCode();
            Products = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>() ?? new List<BranchProductDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching brand: {ex.Message}");
            ErrorMessage = "Failed to load brand details. Please try again later.";
        }
    }
    private async Task OnBranchChanged(BranchOption newBranch)
    {
        Branch = newBranch;
        if (productTable is not null)
        {
            await productTable.ReloadServerData();
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
    protected async Task LoadCategoriesAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/productcategory/all");
            response.EnsureSuccessStatusCode();
            Categories = await response.Content.ReadFromJsonAsync<List<ProductCategoryDto>>() ?? new List<ProductCategoryDto>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching categories: {ex.Message}");
            ErrorMessage = "Failed to load categories. Please try again later.";
        }
    }


    protected async Task AddProduct()
    {
        var dialogOptions = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            BackdropClick = false
        };
        var dialogRef = await DialogService.ShowAsync<CreateProduct>(
            "Add Product",
            new DialogParameters {
                ["BrandId"] = BrandId ,
                ["Branch"] = Branch
            }, dialogOptions);

        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled && result.Data is ProductDto)
            {
                await LoadProductsAsync();             // refresh Count
                await productTable!.ReloadServerData(); // reload table items
                StateHasChanged();
            }
        }
    }


    protected async Task UpdateProduct(int productId)
    {
        var dialogRef = await DialogService.ShowAsync<EditProduct>("Update Product", new DialogParameters { ["ProductId"] = productId });
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                await productTable!.ReloadServerData();
                StateHasChanged();
            }
        }
    }

    protected async Task DeleteProduct(int productId)
    {
        var dialogRef = await DialogService.ShowAsync<DeleteProduct>("Delete Product", new DialogParameters { ["ProductId"] = productId });
        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;
            if (result is not null && !result.Canceled)
            {
                await LoadProductsAsync();
                await productTable!.ReloadServerData();
                StateHasChanged();
            }
        }

    }
}
