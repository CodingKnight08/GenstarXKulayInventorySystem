using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class UpdateProductStocks
{
    [Parameter] public int BrandId { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    [Inject] private ILogger<UpdateProductStocks> Logger { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    private List<BranchProductDto> BranchProducts { get; set; } = new();
    private ProductBrandDto Brand { get; set; } = new();
    private bool IsLoading { get; set; } = false;
    private bool IsSaving { get; set; } = false;
    private BranchOption BranchOptionValue { get; set; }
    private List<BreadcrumbItem> _items { get; set; } = new List<BreadcrumbItem>();
    
    protected override async Task OnParametersSetAsync()
    {
        BranchOptionValue = UserState.Branch.GetValueOrDefault();
        IsLoading = true;
        _items = new List<BreadcrumbItem>
        {
            new ("Brands", href: $"/productbrands"),
            new BreadcrumbItem("View Brand", href:$"/productbrand/{BrandId}"),
            new ("Update Stocks", href: "#", disabled: true)
        };
        await LoadBrand();
        await LoadProducts();
        IsLoading = false;
    }

    protected async Task LoadProducts()
    {
        
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/products/by/{BrandId}/{BranchOptionValue}");
            if(response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<BranchProductDto>>();
                BranchProducts = products;
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading products for brand {BrandId} and branch {Branch}", BrandId, BranchOptionValue);
            SnackBar.Add("An error occurred while loading products. Please try again later.", Severity.Error);
        }
        
    }

    protected async Task LoadBrand()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/productbrand/{BrandId}");
            response.EnsureSuccessStatusCode();
            var brand = await response.Content.ReadFromJsonAsync<ProductBrandDto>();
            if (brand != null)
            {
                Brand = brand;
            }
            else
            {
                SnackBar.Add("Brand details could not be loaded. Please try again later.", Severity.Warning);
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading brand details for brand {BrandId}", BrandId);
            SnackBar.Add("An error occurred while loading brand details. Please try again later.", Severity.Error);
        }
    }

    protected async Task UpdateStocksAsync()
    {
        bool? confirmed = await DialogService.ShowMessageBox(
            title: "Confirm Update",
            message: "Are you sure you want to update the stocks for this brand?",
            yesText: "Update",
            cancelText: "Cancel",
            options: new DialogOptions
            {
                MaxWidth = MaxWidth.Small,
                FullWidth = true,
                CloseButton = true
            });

        if (confirmed != true)
            return; // ❌ user cancelled or closed dialog

        IsSaving = true;

        try
        {
            var payload = BranchProducts.Select(p => new UpdateBranchProductDto
            {
                BranchProductId = p.Id,
                BufferStock = p.BufferStocks,
                ActualQuantity = p.ActualQuantity
            }).ToList();

            var response = await HttpClient.PutAsJsonAsync(
                "api/product/update-stocks",
                payload);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Stocks updated successfully.", Severity.Success);
                NavigationManager.NavigateTo($"productbrand/{BrandId}");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Logger.LogError("Update stocks failed: {Error}", error);
                SnackBar.Add("Failed to update stocks.", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating product stocks");
            SnackBar.Add("Unexpected error while updating stocks.", Severity.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }




    private void Cancel()
    {
        NavigationManager.NavigateTo($"productbrand/{BrandId}");
    }
}
