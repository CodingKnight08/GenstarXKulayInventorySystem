using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.PurchaseOrderBillings;

public partial class GetAllPurchaseOrderBilling
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<GetAllPurchaseOrderBilling> Logger { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    protected List<PurchaseOrderBillingDto> PurchaseOrderBilling { get; set; } = new List<PurchaseOrderBillingDto>();
    protected bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadPurchaseOrderBillings();
    }

    protected async Task LoadPurchaseOrderBillings()
    {
        IsLoading = true;
        try
        {
           var response = await HttpClient.GetAsync("api/billings/purchase-orders/all");
            if (response.IsSuccessStatusCode)
            {
                PurchaseOrderBilling = await response.Content.ReadFromJsonAsync<List<PurchaseOrderBillingDto>>() ?? new List<PurchaseOrderBillingDto>();
            }
            else
            {
                Logger.LogWarning("Failed to load purchase order billings. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading purchase order billings");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ViewPurchaseOrderBilling(int Id)
    {
        NavigationManager.NavigateTo($"/billings/puchase-order-billings/view/{Id}");
    }
}
