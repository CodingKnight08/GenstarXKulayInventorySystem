using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrders;

public partial class EditPurchaseOrders
{
    [Parameter] public PurchaseOrderDto PurchaseOrder { get; set; } = new PurchaseOrderDto();
    [Parameter] public EventCallback<PurchaseOrderDto> OnUpdate { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<EditPurchaseOrders> Logger { get; set; } = default!;
    protected List<SupplierDto> Suppliers { get; set; } = new List<SupplierDto>();
    protected SupplierDto NewSupplier { get; set; } = new SupplierDto();
    protected string SupplierName { get; set; } = string.Empty;


    protected bool IsLoading { get; set; } = true;
    protected bool IsNewSupplier { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        if (PurchaseOrder is null)
        {
            Snackbar.Add("Purchase Order is null", Severity.Error);
            
        }
        else
        {
            SupplierName = PurchaseOrder.Supplier?.SupplierName ?? string.Empty;
        }
        await LoadSuppliers();
        
    }
    protected async Task UpdatePurchaseOrder()
    {
        try
        {
            var selectedSupplier = Suppliers.FirstOrDefault(s => s.SupplierName.Equals(SupplierName, StringComparison.OrdinalIgnoreCase));
           
            if(selectedSupplier == null)
            {
                Snackbar.Add("Please select a valid supplier.", Severity.Error);
                return;
            }
            PurchaseOrder.SupplierId = selectedSupplier.Id;
            PurchaseOrder.Supplier = selectedSupplier;

            if (PurchaseOrder.PurchaseOrderItems != null && PurchaseOrder.PurchaseOrderItems.Any())
            {
                int totalItems = PurchaseOrder.PurchaseOrderItems.Count;
                int receivedCount = PurchaseOrder.PurchaseOrderItems.Count(i => i.IsRecieved);

                if (receivedCount == 0)
                {
                    PurchaseOrder.PurchaseRecieveOption = PurchaseRecieveOption.Pending;
                }
                else if (receivedCount == totalItems)
                {
                    PurchaseOrder.PurchaseRecieveOption = PurchaseRecieveOption.RecieveAll;
                }
                else
                {
                    PurchaseOrder.PurchaseRecieveOption = PurchaseRecieveOption.PartialRecieve;
                }
            }
            else
            {
                PurchaseOrder.PurchaseRecieveOption = PurchaseRecieveOption.Pending;
            }
            var response = await HttpClient.PutAsJsonAsync($"api/purchaseorder/{PurchaseOrder.Id}", PurchaseOrder);
            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Purchase order updated successfully.", Severity.Success); 
                
                if (PurchaseOrder.PurchaseOrderItems == null || !PurchaseOrder.PurchaseOrderItems.Any())
                {
                    Snackbar.Add("No items in the purchase order to update.", Severity.Warning);
                    return;
                }
                else
                {
                    var updatedOrderItems = await HttpClient.PutAsJsonAsync($"api/purchaseorderitem/update-items", PurchaseOrder.PurchaseOrderItems);
                    if (updatedOrderItems.IsSuccessStatusCode)
                    {
                        Snackbar.Add("Purchase order items updated successfully.", Severity.Success);
                        await OnUpdate.InvokeAsync(PurchaseOrder);
                    }
                }
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Error updating purchase order: {error}", Severity.Error);
                Logger.LogError($"Error updating purchase order: {error}");
                return;
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error updating purchase order: {ex.Message}", Severity.Error);
            Logger.LogError($"Error updating purchase order: {ex}");
            return;
        }
        
    }

    protected async Task CancelEdit()
    {
        await OnUpdate.InvokeAsync(PurchaseOrder);
    }

    protected Task<IEnumerable<string>> SearchSupplier(string value, CancellationToken token)
    {
        var matches = Suppliers
            .Where(s => s.SupplierName.Contains(value, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.SupplierName);

        return Task.FromResult(matches);
    }
    protected void OnSupplierChanged(string value)
    {
        SupplierName = value;

        if (string.IsNullOrWhiteSpace(SupplierName))
        {
            IsNewSupplier = false;
            return;
        }

        var exists = Suppliers.Any(s =>
            s.SupplierName.Equals(SupplierName, StringComparison.OrdinalIgnoreCase));

        IsNewSupplier = !exists;
    }
    protected async Task ConfirmAddSupplier()
    {
        bool? result = await DialogService.ShowMessageBox(
            "Add New Supplier?",
            $"Supplier '{SupplierName}' not found. Do you want to add it?",
            yesText: "Yes", noText: "No");

        if (result == true)
        {
            await AddNewSupplier();
        }
    }
    protected async Task AddNewSupplier()
    {

        NewSupplier.SupplierName = SupplierName;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/supplier/create-with-return", NewSupplier);

            if (response.IsSuccessStatusCode)
            {
                var created = await response.Content.ReadFromJsonAsync<SupplierDto>();
                if (created is not null)
                {
                    Suppliers.Add(created);
                    SupplierName = created.SupplierName;
                    Snackbar.Add("Supplier added successfully.", Severity.Success);
                    IsNewSupplier = false;
                    StateHasChanged();
                }
            }
            else
            {
                Snackbar.Add("Failed to add supplier.", Severity.Error);
            }


        }
        catch (Exception ex)
        {
            Snackbar.Add("Failed to add supplier.", Severity.Error);
            Logger.LogError($"Failed to add supplier: {ex.Message}", Severity.Error);
        }
    }

    protected async Task LoadSuppliers()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/supplier/all");
            response.EnsureSuccessStatusCode();
            var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
            Suppliers = suppliers ?? new List<SupplierDto>();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Error loading suppliers: {ex.Message}", Severity.Error);
            Logger.LogError($"Error loading suppliers: {ex}");
        }
    }

    protected void OnDateChanged(DateTime? newDate)
    {
        if (newDate.HasValue)
        {
            PurchaseOrder.PurchaseOrderDate = newDate.Value;
        }
    }

    protected void OnExpectedDateChanged(DateTime? expectedDate)
    {
        if (expectedDate.HasValue)
        {
            PurchaseOrder.ExpectedDeliveryDate = expectedDate.Value;
        }
    }

    protected void HandleUpdatedPurchaseOrderItems(List<PurchaseOrderItemDto> updatedPurchaseORderItems)
    {
        PurchaseOrder.PurchaseOrderItems = updatedPurchaseORderItems;
        StateHasChanged();
    }
}
