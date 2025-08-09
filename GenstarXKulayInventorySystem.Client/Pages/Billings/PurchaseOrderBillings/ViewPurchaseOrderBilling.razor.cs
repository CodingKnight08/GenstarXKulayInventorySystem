using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Billings.PurchaseOrderBillings;

public partial class ViewPurchaseOrderBilling
{
    [Parameter] public int Id { get; set; }
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected ILogger<ViewPurchaseOrderBilling> Logger { get; set; } = default!;
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected PurchaseOrderBillingDto PurchaseOrderBilling { get; set; } = new PurchaseOrderBillingDto();
    protected List<PurchaseOrderItemDto> PurchaseOderItems { get; set; } = new List<PurchaseOrderItemDto>();
    protected bool IsLoading { get; set; } = true;

    protected List<BreadcrumbItem> _items =
    [
        new("Billings", href: "/billings"),
        new("View Purchase Order Bill", href: null, disabled: true)
    ];
    protected override async Task OnInitializedAsync()
    {
        await LoadPurchaseOrderBilling();
        await LoadPurchaseOrderItems();
    }

    protected async Task LoadPurchaseOrderBilling()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/billings/purchase-orders/{Id}");
            if (response.IsSuccessStatusCode)
            {
                PurchaseOrderBilling = await response.Content.ReadFromJsonAsync<PurchaseOrderBillingDto>() ?? new PurchaseOrderBillingDto();
            }
            else
            {
                Logger.LogWarning("Failed to load purchase order billing. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading purchase order billing");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task LoadPurchaseOrderItems()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/purchaseorderitem/{PurchaseOrderBilling.PurchaseOrderId}/items/received");
            if (response.IsSuccessStatusCode)
            {
                PurchaseOderItems = await response.Content.ReadFromJsonAsync<List<PurchaseOrderItemDto>>() ?? new List<PurchaseOrderItemDto>();

            }
            else
            {
                Logger.LogError("Error occured");
            }
        }
        catch(Exception ex)
        {
            Logger.LogError($"{ex.Message}", ex);
        }
        IsLoading = false;
    }

    protected async Task UpdateBilling()
    {
        var dialog = await DialogService.ShowAsync<EditPurchaseOrderBilling>("",
            new DialogParameters
            {
                {"PurchaseOrderBilling",PurchaseOrderBilling }
            },
            new DialogOptions
            {
                MaxWidth=MaxWidth.Medium,
                FullWidth = true,
                CloseButton = true,
                BackdropClick = false
            });
        if(dialog is not null)
        {
            var result = await dialog.Result;
            if(result is not null && !result.Canceled)
            {
                await LoadPurchaseOrderBilling();
                StateHasChanged();
            }
        }
    }
    protected int TermsOption(PaymentTermsOption term)
    {
        return term switch
        {
            PaymentTermsOption.Today => 0,
            PaymentTermsOption.SevenDays => 7,
            PaymentTermsOption.ThirtyDays => 30,
            PaymentTermsOption.SixtyDays => 60,
            PaymentTermsOption.NinetyDays => 90,
            PaymentTermsOption.Custom => PurchaseOrderBilling.CustomPaymentTermsOption ?? 0, 
            _ => 0
        };
    }
}
