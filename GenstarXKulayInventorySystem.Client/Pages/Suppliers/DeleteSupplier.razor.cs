using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Suppliers;

public partial class DeleteSupplier
{
    [Parameter] public int SupplierId { get; set; }
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] public IMudDialogInstance DialogService { get; set; } = default!;

    protected SupplierDto DeletedSupplier { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadSupplierAsync();
    }

    protected async Task LoadSupplierAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.GetAsync($"api/supplier/{SupplierId}");
            response.EnsureSuccessStatusCode();
            DeletedSupplier = await response.Content.ReadFromJsonAsync<SupplierDto>() ?? new SupplierDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching supplier: {ex.Message}");
            ErrorMessage = "Failed to load supplier. Please try again later.";
        }
        IsLoading = false;
    }

    protected async Task DeleteSupplierAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        try
        {
            var response = await HttpClient.DeleteAsync($"api/supplier/{SupplierId}");
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Supplier deleted successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                ErrorMessage = "Failed to delete supplier. Please try again later.";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error deleting supplier: {ex.Message}");
            ErrorMessage = "Failed to delete supplier. Please try again later.";
        }
        IsLoading = false;
    }
    protected void Cancel()
    {
        DialogService.Close(DialogResult.Cancel());
    }
}
