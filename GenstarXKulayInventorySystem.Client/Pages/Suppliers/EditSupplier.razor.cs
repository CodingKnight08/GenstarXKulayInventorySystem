using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Suppliers;

public partial class EditSupplier
{
    [Parameter] public int SupplierId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    protected SupplierDto UpdatedSupplier { get; set; } = new();
    protected bool IsLoading { get; set; } = true;
    protected bool IsUpdating { get; set; } = false;
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
            UpdatedSupplier = await response.Content.ReadFromJsonAsync<SupplierDto>() ?? new SupplierDto();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching supplier: {ex.Message}");
            ErrorMessage = "Failed to load supplier. Please try again later.";
        }
        await Task.Delay(2000);
        IsLoading = false;
    }
    protected async Task UpdateSupplierAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        IsUpdating = true;
        try
        {
            var response = await HttpClient.PutAsJsonAsync($"api/supplier/{SupplierId}", UpdatedSupplier);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Supplier updated successfully!", Severity.Success);
                DialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error updating supplier: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error updating supplier: {ex.Message}");
            Snackbar.Add("Failed to update supplier. Please try again later.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            IsUpdating = false;
        }
    }

    protected void Cancel()
    {
        DialogService.Close(DialogResult.Cancel());
    }
}