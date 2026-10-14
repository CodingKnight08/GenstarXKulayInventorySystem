using GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.BranchProduct;
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
    [Parameter, SupplyParameterFromQuery(Name = "pageskip")]
    public int PageSkip { get; set; } = 0;
    [Parameter, SupplyParameterFromQuery(Name = "pagetake")]
    public int PageTake { get; set; } = 10;
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [Inject] protected ILogger<ViewBrand> Logger { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected ProductBrandDto Brand { get; set; } = new ProductBrandDto();
    protected List<BranchProductDto> Products { get; set; } = new List<BranchProductDto>();
    protected List<ProductCategoryDto> Categories { get; set; } = new List<ProductCategoryDto>();
    protected BranchOption Branch { get; set; }
    protected MudTable<BranchProductDto>? productTable;
    protected bool IsLoading = false;
    protected string? ErrorMessage { get; set; }
    protected List<BreadcrumbItem> items = new();
    protected string SearchTerm { get; set; } = string.Empty;
    protected int CurrentPage { get; set; }
    protected int Count { get; set; }
    protected int Skip { get; set; }
    protected int Take { get; set; }
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
            new("Brands", href: $"/productbrands?pageskip={PageSkip}&pagetake={PageTake}"),
            new("Brand Detail", href: "#", disabled: true),
        ];
        
        await Task.Delay(1000);

        IsLoading = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && productTable != null)
        {
            CurrentPage = PageSkip / PageTake;

            productTable.NavigateTo(CurrentPage);
        }
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
            int skip = state.Page * state.PageSize;
            int take = state.PageSize;

            var url = $"api/product/by/{BrandId}/{Branch}?skip={skip}&take={take}&search={SearchTerm}";

            Logger.LogInformation($"Loading products → {url}");

            var response = await HttpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new TableData<BranchProductDto>
                {
                    Items = new List<BranchProductDto>(),
                    TotalItems = 0
                };
            }

            var result = await response.Content.ReadFromJsonAsync<BranchProductPageResultDto<BranchProductDto>>(cancellationToken);

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

        if (productTable != null)
            await productTable.ReloadServerData();
    }
    protected void OnPageChanged(int page)
    {
        CurrentPage = page;
        Skip = CurrentPage * Take;
        StateHasChanged();
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
            BackdropClick = false,
           
        };
        var dialogRef = await DialogService.ShowAsync<CreateProduct>(
            "Add Product",
            new DialogParameters {
                ["BrandId"] = BrandId ,
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
    private async Task OnSearchChanged(string text)
    {
        SearchTerm = text;

        if (productTable is not null)
            await productTable.ReloadServerData();
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

    protected void UpdateStocks()
    {
        NavigationManager.NavigateTo($"/productbrands/update-stocks/{BrandId}");

    }
    protected async Task AssignStaff(int id)
    {
        var dialogParameters = new DialogParameters
        {
            ["BranchProductId"] = id
        };
        var dialogOptions = new DialogOptions
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Medium,
            BackdropClick = false
        };
        var dialog = await DialogService.ShowAsync<BranchProductStaff>("Assign Staff", dialogParameters, dialogOptions);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadBranchProducts();            
            await productTable!.ReloadServerData();
            StateHasChanged();
        }
    }
    protected async Task CreateBranchProduct()
    {
        var dialogParameters = new DialogParameters
        {
            ["BrandId"] = BrandId,
            ["Branch"] = Branch,
        };

        var dialogOptions = new DialogOptions
        {
            FullWidth = true,
            MaxWidth = MaxWidth.Medium,
            BackdropClick = false,
        };

        var dialog = await DialogService.ShowAsync<CreateBranchProduct>(
            "Create Branch Product",
            dialogParameters,
            dialogOptions);

        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            await LoadBranchProducts();            // refresh existing list (important for your logic)
            await productTable!.ReloadServerData(); // reload MudTable server data
            StateHasChanged();
        }
    }
}
