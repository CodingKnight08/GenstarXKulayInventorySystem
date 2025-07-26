using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductCategory;

public partial class CreateProductCategory
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    protected ProductCategoryDto NewCategory { get; set; } = new ProductCategoryDto();
    
    protected string? ErrorMessage { get; set; }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }
    protected async Task CreateCategory()
    {
      
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/productcategory", NewCategory);
            response.EnsureSuccessStatusCode();
            if(response.IsSuccessStatusCode)
            {
                Snackbar.Add("Product category created successfully!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(NewCategory));
            }
            else
            {
                ErrorMessage = "Failed to create product category. Please try again later.";
                return;
            }
           
        }
        catch (Exception ex)
        {
            ErrorMessage = "Failed to create product category. Please try again later.";
            Console.Error.WriteLine($"Error creating product category: {ex.Message}");
        }
      
    }
}
