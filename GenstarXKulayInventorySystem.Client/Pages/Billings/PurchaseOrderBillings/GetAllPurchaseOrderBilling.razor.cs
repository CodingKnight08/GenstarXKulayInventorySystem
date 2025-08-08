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
    protected List<BillingDto> PurchaseOrderBilling { get; set; } = new List<BillingDto>();
    protected bool IsLoading { get; set; } = true;

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    protected async Task LoadPurchaseOrderBillings()
    {
        IsLoading = true;
        try
        {
           var response = await HttpClient.GetAsync("api/billings/all/purchaseorders");
            if (response.IsSuccessStatusCode)
            {
                PurchaseOrderBilling = await response.Content.ReadFromJsonAsync<List<BillingDto>>() ?? new List<BillingDto>();
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

    protected async Task CreatePurchaseOrderBilling()
    {
        try
        {
            var dialog = await DialogService.ShowAsync<CreatePurchaseOrderBilling>("",
                new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth=true}
                );
            if(dialog is not null)
            {
                var result = await dialog.Result;
                if (result is not null && !result.Canceled && result.Data is BillingDto)
                {
                    
                    await LoadPurchaseOrderBillings();
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
           Logger.LogError(ex, "Error creating purchase order billing");
        }
    }
}
