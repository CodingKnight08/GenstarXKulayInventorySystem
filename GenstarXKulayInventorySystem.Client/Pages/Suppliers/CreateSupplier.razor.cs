using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Suppliers;

public partial class CreateSupplier
{
    [Inject] public HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogService { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    protected SupplierDto NewSupplier { get; set; } = new();
    protected string? ErrorMessage { get; set; }
    protected async Task CreateSupplierAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewSupplier.SupplierName))
        {
            try
            {
                var response = await HttpClient.PostAsJsonAsync("api/supplier", NewSupplier);
                if (response.IsSuccessStatusCode)
                {
                    Snackbar.Add("Supplier created successfully!", Severity.Success);
                    DialogService.Close(DialogResult.Ok(NewSupplier));
                }
                else
                {
                    ErrorMessage = "Failed to create supplier. Please try again.";
                    Snackbar.Add($"Failed to add supplier", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
            }
        }
        else
        {
            ErrorMessage = "Supplier Name and Contact Number are required.";
        }
    }

    protected void Cancel()
    {
        DialogService.Cancel();
    }
}