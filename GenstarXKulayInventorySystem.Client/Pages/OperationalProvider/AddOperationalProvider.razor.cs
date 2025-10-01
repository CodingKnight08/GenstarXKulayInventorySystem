using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.OperationalProvider;

public partial class AddOperationalProvider
{
    [Parameter] public OperationsProviderDto? Provider { get; set; }
    [Parameter] public BranchOption Branch { get; set; }
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<AddOperationalProvider> Logger { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    
    private OperationsProviderDto NewProvider { get; set; } = new();
    private bool IsValid => string.IsNullOrWhiteSpace(NewProvider.ProviderName) || NewProvider.ProviderName.Length <= 3 || string.IsNullOrEmpty(NewProvider.TINNumber) || string.IsNullOrEmpty(NewProvider.Address);

    protected override void OnParametersSet()
    {
        
        NewProvider.Branch = Branch;
        if (Provider != null)
        {
            NewProvider.ProviderName = Provider.ProviderName;
        }
    }
    protected async Task Submit()
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/operationsprovider", NewProvider);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Operational provider added successfully!", Severity.Success);
                MudDialog.Close(DialogResult.Ok(NewProvider));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed to add operational provider: {error}", Severity.Error);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError($"Error adding new operational provider: {ex.Message}");
            Snackbar.Add("Failed to add operational provider", Severity.Error);
        }
    }

    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
