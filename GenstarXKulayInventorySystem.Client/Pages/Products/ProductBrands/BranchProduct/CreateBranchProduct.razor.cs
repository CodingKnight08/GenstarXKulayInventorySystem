using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.BranchProduct;

public partial class CreateBranchProduct
{
    [Parameter] public int BrandId { get; set; }
    [Parameter] public BranchOption Branch { get; set; }
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;
    [Inject] private ILogger<CreateBranchProduct> Logger { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    private List<GlobalProductDto> NotYetAdded { get; set; } = new List<GlobalProductDto>();
    private GlobalProductDto? SelectedMasterProduct { get; set; } = new GlobalProductDto();
    private BranchProductDto? NewBranchProduct { get;set; } = new BranchProductDto();
    private bool IsLoading { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        try
        {
            await LoadNotInBranchProduct();
        }
        catch(Exception ex)
        {

        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task LoadNotInBranchProduct()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/product/all/global/notexisting/{BrandId}/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<GlobalProductDto>>();
                NotYetAdded = products;
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error loading not existing products for brand {BrandId} and branch {Branch}", BrandId, Branch);
            SnackBar.Add("An error occurred while loading products. Please try again later.", Severity.Error);
        }
    }
    protected Task<IEnumerable<GlobalProductDto>> SearchMasterProduct(string value, CancellationToken cancellationToken)
    {
        IEnumerable<GlobalProductDto> result = NotYetAdded ?? Enumerable.Empty<GlobalProductDto>();

        if (!string.IsNullOrWhiteSpace(value))
        {
            result = result.Where(p =>
                (p.ProductName ?? string.Empty).Contains(value, StringComparison.OrdinalIgnoreCase));
        }

        return Task.FromResult(result.DistinctBy(p => p.Id));
    }

    protected async Task OnGlobalProductSelect(GlobalProductDto product)
    {
        if(product is null)
        {
            SelectedMasterProduct = null;
            NewBranchProduct = null;
            return;
        }
        SelectedMasterProduct = product;
        NewBranchProduct.MasterProductId = product.Id;
    }

    protected async Task Submit()
    {
        if (SelectedMasterProduct is null || NewBranchProduct is null)
        {
            SnackBar.Add("Please select a product.", Severity.Warning);
            return;
        }

        try
        {
            IsLoading = true;

            // ensure required fields
            NewBranchProduct.MasterProductId = SelectedMasterProduct.Id;
            NewBranchProduct.Branch = Branch;
            NewBranchProduct.Size = 1;

            var response = await HttpClient.PostAsJsonAsync("api/product/branch", NewBranchProduct);

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<BranchProductDto>();

                SnackBar.Add("Product added to branch successfully.", Severity.Success);

                Dialog.Close(DialogResult.Ok(created ?? NewBranchProduct));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Logger.LogWarning("CreateBranchProduct failed: {Error}", error);
                SnackBar.Add($"Failed to add product: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating branch product");
            SnackBar.Add("An unexpected error occurred.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Cancel()
    {
        Dialog.Cancel();
    }
}
