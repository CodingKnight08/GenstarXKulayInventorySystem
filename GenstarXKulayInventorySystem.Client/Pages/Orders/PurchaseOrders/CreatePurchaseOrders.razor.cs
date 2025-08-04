using GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrderItems;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Net.Http.Json;
using static MudBlazor.Colors;

namespace GenstarXKulayInventorySystem.Client.Pages.Orders.PurchaseOrders;

public partial class CreatePurchaseOrders
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialogService { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ILogger<CreatePurchaseOrders> Logger { get; set; } = default!;

    protected GetAllToBeAddedPurchaseOrderItems PurchaseOrderItemComponent { get; set; } = default!;
    protected List<SupplierDto> Suppliers { get; set; } = new List<SupplierDto>();
    protected List<PurchaseOrderItemDto> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItemDto>();
    protected SupplierDto NewSupplier { get; set; } = new SupplierDto();
    protected DateTime MinDate { get; set; } = new DateTime(DateTime.Now.Year, 1, 1);
    protected PurchaseOrderDto NewPurchaseOrder { get; set; } = new();
    protected string SupplierName { get; set; } = string.Empty;
    protected bool IsLoading { get; set; } = true;
    protected bool IsNewSupplier { get; set; } = false;
    protected MudForm _form { get; set; } = default!;

    protected bool IsPurchaseOrderValid => !string.IsNullOrWhiteSpace(SupplierName) && !string.IsNullOrWhiteSpace(NewPurchaseOrder.PurchaseOrderNumber) && NewPurchaseOrder.PurchaseOrderItems.Count !=0;
    protected override  async Task OnInitializedAsync()
    {
        IsLoading = true;
        await LoadSuppliers();
        IsLoading = false;
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
        catch(Exception ex)
        {
            Snackbar.Add("Add Purchase Order Failed", Severity.Error);
            Logger.LogError($"An error occur {ex}", Severity.Error);
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

    protected async Task Submit()
    {
        IsLoading = true;

        try
        {

            var selectedSupplier = Suppliers
                .FirstOrDefault(s => s.SupplierName.Equals(SupplierName, StringComparison.OrdinalIgnoreCase));

            if (selectedSupplier == null)
            {
                Snackbar.Add("Supplier not found. Please add the supplier first.", Severity.Error);
                IsLoading = false;
                return;
            }

            NewPurchaseOrder.SupplierId = selectedSupplier.Id;
            NewPurchaseOrder.PurchaseOrderDate = NewPurchaseOrder.PurchaseOrderDate == default
                ? DateTime.Now
                : NewPurchaseOrder.PurchaseOrderDate;
            NewPurchaseOrder.PurchaseOrderItems = PurchaseOrderItems;
            var response = await HttpClient.PostAsJsonAsync("api/purchaseorder", NewPurchaseOrder);

            if (response.IsSuccessStatusCode)
            {
                Snackbar.Add("Purchase order created successfully.", Severity.Success);
                MudDialogService.Close(DialogResult.Ok(true));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Snackbar.Add($"Failed to create purchase order: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Snackbar.Add("An unexpected error occurred.", Severity.Error);
            Logger.LogError($"Error occurred: {ex}", Severity.Error);
        }

        IsLoading = false;
    }



    protected Task<IEnumerable<string>> SearchSupplier(string value, CancellationToken token)
    {
        var matches = Suppliers
            .Where(s => s.SupplierName.Contains(value, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.SupplierName);

        return Task.FromResult(matches);
    }
    protected async Task OnSupplierChanged(string value)
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

    protected void Cancel()
    {
        MudDialogService.Cancel();
    }

    protected void OnDateChanged(DateTime? newDate)
    {
        if (newDate.HasValue)
        {
            NewPurchaseOrder.PurchaseOrderDate = newDate.Value;
        }
    }

    protected void OnExpectedDateChanged(DateTime? expectedDate)
    {
        if (expectedDate.HasValue)
        {
            NewPurchaseOrder.ExpectedDeliveryDate = expectedDate.Value;
        }
    }

    protected Task HandlePurchaseOrderItemsChanged(List<PurchaseOrderItemDto> items)
    {
        PurchaseOrderItems = items;
        NewPurchaseOrder.PurchaseOrderItems = PurchaseOrderItems;
        return Task.CompletedTask;
    }
}
