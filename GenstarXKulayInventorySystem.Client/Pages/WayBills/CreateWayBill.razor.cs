using GenstarXKulayInventorySystem.Client.Pages.WayBills.WayBillItems;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills;

public partial class CreateWayBill
{
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ISnackbar SnackBar { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private ILogger<CreateWayBill> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    public GetAllWayBillItems WayBillItemsComponent { get; set; } = default!;

    protected WayBillDto WayBill { get; set; } = new WayBillDto();
    protected List<SupplierDto> Suppliers { get; set; } = new List<SupplierDto>();
    protected List<ProductBrandDto> ProductBrands { get; set; } = new List<ProductBrandDto>();
    protected ProductBrandDto? SelectedBrand { get; set; } = new ProductBrandDto();
    protected string SupplierName { get; set; } = string.Empty;
    protected bool IsLoading { get; set; } = false;
    protected bool IsSupplierLoading { get; set; } = false;
    protected bool HasBrand => SelectedBrand != null && SelectedBrand.Id > 0;
    protected bool IsValid =>
        HasBrand &&
        (WayBillItemsComponent?.WayBillItems?.Any() ?? false);



    protected string SupplierUrl = "api/supplier";
    protected BranchOption Branch { get; set; }

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        Branch = UserState.Branch.GetValueOrDefault();
        await LoadSupplier();
        await LoadBrands();
        IsLoading = false;
    }
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            StateHasChanged(); // forces parent to recompute IsValid after @ref is assigned
        }
        return base.OnAfterRenderAsync(firstRender);
    }


    protected async Task LoadSupplier()
    {
        IsSupplierLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"{SupplierUrl}/all/{Branch}");
            if (response.IsSuccessStatusCode)
            {
                var suppliers = await response.Content.ReadFromJsonAsync<List<SupplierDto>>();
                if(suppliers != null)
                {
                    Suppliers = suppliers;
                }
                else
                {
                    SnackBar.Add("Add Suppliers First!", Severity.Warning);
                }
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error occur loading suppliers!");
        }
        finally
        {
            IsSupplierLoading = false;
        }
    }

    protected Task<IEnumerable<string>> SearchSupplier(string value, CancellationToken cancellationToken)
    {
        if (Suppliers is null || !Suppliers.Any())
            return Task.FromResult(Enumerable.Empty<string>());

        var result = Suppliers.Where(s => !string.IsNullOrWhiteSpace(s.SupplierName) && (string.IsNullOrWhiteSpace(value) || s.SupplierName.Contains(value, StringComparison.OrdinalIgnoreCase)))
            .Select(s => s.SupplierName!); ;
        return Task.FromResult(result);
    }
    protected void OnSupplierNameType(string supplier)
    {
        SupplierName = supplier;
        var matchSupplier = Suppliers.FirstOrDefault(s => s.SupplierName.Equals(supplier, StringComparison.OrdinalIgnoreCase));
        if(matchSupplier != null)
        {
            WayBill.SupplierId = matchSupplier.Id;
        }
    }
    protected async Task LoadBrands()
    {
        try
        {
            var response = await HttpClient.GetAsync("api/productbrand/all/brandnames");
            if (response.IsSuccessStatusCode)
            {
                var brands = await response.Content.ReadFromJsonAsync<List<ProductBrandDto>>();
                if (brands != null)
                {
                    ProductBrands = brands;
                }
            }
            else
            {
               SnackBar.Add("Failed to load product brands.", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message, "Error occur loading brands!");
        }
    }
    protected Task<IEnumerable<ProductBrandDto>> SearchBrands(string value, CancellationToken cancellationToken)
    {
        if (ProductBrands == null || ProductBrands.Count == 0)
            return Task.FromResult(Enumerable.Empty<ProductBrandDto>());

        var query = value?.Trim() ?? string.Empty;

        var result = ProductBrands
            .Where(b => string.IsNullOrWhiteSpace(value) ||
            b.BrandName.Contains(query, StringComparison.OrdinalIgnoreCase))
            .GroupBy(b => b.Id)
            .Select(g => g.First());


        return Task.FromResult(result);
    }

    protected async Task OnBrandSelect(ProductBrandDto brand)
    {
        if(brand is null)
        {
            SelectedBrand = null;
            return;
        }
        SelectedBrand = brand;
        StateHasChanged();
    }
    protected void Cancel()
    {
        NavigationManager.NavigateTo("/waybills");
    }

    protected async Task SubmitWayBill()
    {
        // Get items from the child component
        var itemsFromChild = WayBillItemsComponent?.WayBillItems ?? new List<WayBillItemsDto>();


        if (!itemsFromChild.Any())
        {
            SnackBar.Add("Add at least one item to submit the way bill.", Severity.Warning);
            return;
        }

        // Clone items and nullify BranchProduct to avoid circular reference
        var itemsToSubmit = itemsFromChild
            .Select(item => new WayBillItemsDto
            {
                Id = item.Id,
                BranchProductId = item.BranchProductId,
                Quantity = item.Quantity,
                ItemPrice = item.ItemPrice,
                TotalPrice = item.TotalPrice,
                BranchProduct = null
            })
            .ToList();

        WayBill.WayBillItems = itemsToSubmit;

        try
        {
            var response = await HttpClient.PostAsJsonAsync("api/waybill/create", WayBill);
            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Way bill submitted successfully.", Severity.Success);


                NavigationManager.NavigateTo("/waybills");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                SnackBar.Add($"Failed to submit way bill. {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting way bill");
            SnackBar.Add("An error occurred while submitting the way bill.", Severity.Error);
        }
    }

    private void RecalculateIsValid()
    {
        StateHasChanged(); 
    }
}
