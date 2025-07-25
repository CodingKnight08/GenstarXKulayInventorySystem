using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands;

public partial class GetAllProductBrands
{
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected List<ProductBrandDto> ProductBrands { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadProductBrandsAsync();
    }

    private async Task LoadProductBrandsAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await HttpClient.GetAsync("api/productbrand/all");

            response.EnsureSuccessStatusCode();

            var brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>();

            ProductBrands = brands ?? new List<ProductBrandDto>();
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
        var dialogRef = await DialogService.ShowAsync<CreateProductBrand>("Create Product Brand");

        if (dialogRef is not null)
        {
            var result = await dialogRef.Result;

            if (result is not null && !result.Canceled && result.Data is ProductBrandDto)
            {
                await LoadProductBrandsAsync();
                StateHasChanged();
            }
        }
    }




    protected void ViewBrands(int brandId)
    {
        NavigationManager.NavigateTo($"/productbrand/{brandId}");
        
    }
}

