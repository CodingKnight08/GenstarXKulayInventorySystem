using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class GetAllProductBrands
{

    [Parameter, SupplyParameterFromQuery(Name = "pageskip")] public int PageSkip { get; set; } = 0;
    [Parameter, SupplyParameterFromQuery(Name = "pagetake")] public int PageTake { get; set; } = 10;
    [Inject] protected ILogger<GetAllProductBrands> Logger { get; set; } = default!;
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected List<ProductBrandDto> ProductBrands { get; set; } = new();
    protected List<ProductBrandDto> FilteredProductBrands { get; set; } = new List<ProductBrandDto>();
    protected string SearchTerm { get; set; } = string.Empty;   
    protected bool IsLoading { get; set; } = true;
    private MudTable<ProductBrandDto>? brandsTable;
    private int Count { get; set; }

    protected string? ErrorMessage { get; set; }
    private int CurrentPage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {

            await LoadData(); 
        }
        catch (Exception ex)
        {
            Logger.LogError($"An error occured upon initialization: {ex.Message}", Severity.Error);
        }
       
    }
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && brandsTable != null)
        {
            CurrentPage = PageSkip / PageTake;

            brandsTable.NavigateTo(CurrentPage);
        }

        return Task.CompletedTask;
    }
    protected override async Task OnParametersSetAsync()
    {
        if (PageTake <= 0) PageTake = 10; 

        CurrentPage = PageSkip / PageTake;

        ApplyPaging();

        
    }
    protected void OnPageChanged(int page)
    {
        CurrentPage = page;
        PageSkip = CurrentPage * (PageTake > 0 ? PageTake : 10);

        ApplyPaging();
        NavigationManager.NavigateTo(
            $"/productbrands?pageskip={PageSkip}&pagetake={PageTake}",
            forceLoad: false);
    }
    protected void OnRowsPerPageChanged(int newPageSize)
    {
        PageTake = newPageSize;
        CurrentPage = 0; 
        PageSkip = 0;
        ApplyPaging();
    }


    private async Task LoadData()
    {
        IsLoading = true;
        try
        { 
            var response = await HttpClient.GetAsync("api/productbrand/all/brandnames");
            response.EnsureSuccessStatusCode();
            ProductBrands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>() ?? new List<ProductBrandDto>();
            ApplyPaging();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading product brands: {ex.Message}");
            ErrorMessage = "Failed to load product brands. Please try again later.";
        }
        finally
        {
            IsLoading = false;
        }

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
                await LoadData();
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
                await LoadData();
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
                await LoadData();
                StateHasChanged();
            }
        }
    }
    protected void ViewBrands(int brandId)
    {
        NavigationManager.NavigateTo($"/productbrand/{brandId}");
        //NavigationManager.NavigateTo($"/productbrand/{brandId}?pageskip={PageSkip}&pagetake={PageTake}");
    }



    private void SearchBrands(string value)
    {
        SearchTerm = value;

        CurrentPage = 0;
        PageSkip = 0;

        ApplyFiltering();
        ApplyPaging();

        StateHasChanged();
    }
    private void ApplyPaging()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            FilteredProductBrands = ProductBrands;
        }
        else
        {
            FilteredProductBrands = ProductBrands
                .Where(b => b.BrandName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
    private void ApplyFiltering()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            FilteredProductBrands = ProductBrands.ToList();
        }
        else
        {
            FilteredProductBrands = ProductBrands
                .Where(b => b.BrandName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}

