using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductCategory;

public partial class EditProductCategory
{
    [Parameter] public int ProductCategoryId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogService { get; set; } = default!;

    protected ProductCategoryDto UpdatedCategory { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadProductCategoryAsync();
    }

    protected async Task LoadProductCategoryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync($"api/productcategory/{ProductCategoryId}");
            response.EnsureSuccessStatusCode();
            UpdatedCategory = await response.Content.ReadFromJsonAsync<ProductCategoryDto>() ?? new ProductCategoryDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching product category: {ex.Message}");
            ErrorMessage = "Failed to load product category. Please try again later.";
        }
        IsLoading = false;
    }

    protected async Task UpdateProductCategoryAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/productcategory/{ProductCategoryId}", UpdatedCategory);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product category updated successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true)); // Close the dialog with a success result
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error updating product category: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating product category: {ex.Message}");
            Snackbar.Add("Failed to update product category. Please try again later.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void Cancel()
    {
        DialogService.Cancel(); 
    }
}
