using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.OperationalBillings;

public partial class CreateOperationalBilling
{
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    protected BillingDto Billing { get; set; } = new();
    protected string? ErrorMessage { get; set; }
    protected DateTime MinDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
    protected List<OperationsProviderDto> OperationsProviders { get; set; } = new();
    protected OperationsProviderDto NewProvider { get; set; } = new OperationsProviderDto();
    protected string ProviderName { get; set; } = string.Empty;
    protected bool IsLoading { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        Billing.Category = BillingCategory.Logistics;
        Billing.DateOfBilling = DateTime.UtcNow;
        Billing.Branch = UtilitiesHelper.GetBillingBranch(UserState.Branch.GetValueOrDefault());
        await LoadProviders();
    }

    protected async Task LoadProviders()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/operationsproviders/all/{UserState.Branch.GetValueOrDefault()}");
            if (response.IsSuccessStatusCode)
            {
                var providers = await response.Content.ReadFromJsonAsync<List<OperationsProviderDto>>();
                if (providers != null)
                {
                    OperationsProviders = providers;
                }
            }
            else
            {
                ErrorMessage = "Failed to load providers. Please try again.";
                Snackbar.Add(ErrorMessage, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while loading providers: {ex.Message}";
            Snackbar.Add(ErrorMessage, Severity.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    protected Task<IEnumerable<string>> SearchProviders(string value, CancellationToken cancellationToken)
    {
        if(OperationsProviders is null || !OperationsProviders.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        var result = OperationsProviders
            .Where(x => x.ProviderName.Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .Select(x => x.ProviderName);
        return Task.FromResult(result);
    }

    protected void OnProviderNameTyped(string providerName)
    {
        ProviderName = providerName;
        var matchedProvider = OperationsProviders.FirstOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.InvariantCultureIgnoreCase));
        if (matchedProvider != null)
        {
            Billing.OperatationsProviderId = matchedProvider.Id;
        }
        
    }
    protected async Task SubmitAsync()
    {
        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/billings/operational", Billing);
            if (response.IsSuccessStatusCode)
            {
                MudDialog.Close(DialogResult.Ok(true));
            }
            else
            {
                ErrorMessage = "Failed to create billing. Please try again.";
                Snackbar.Add(ErrorMessage, Severity.Error);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
            Snackbar.Add(ErrorMessage, Severity.Error);
        }
    }
    protected void Cancel()
    {
        MudDialog.Cancel();
    }

    protected void OnDateChanged(DateTime? date)
    {
        Billing.DateOfBilling = date ?? DateTime.Today;
    }

    protected bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Billing.BillingName)
        && Billing.Amount != 0;
    }
}
