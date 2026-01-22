using GenstarXKulayInventorySystem.Client.Pages.WayBills.WayBillDamageItems;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills;

public partial class ViewWayBill
{
    [Parameter] public int WayBillId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewWayBill> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected DialogService DialogService { get; set; } = default!;

    protected WayBillDto WayBill { get; set; } = new WayBillDto();
    protected List<WayBillItemsDto> WayBillItems { get; set; } = new List<WayBillItemsDto>();
    protected List<BreadcrumbItem> _items { get; set; } = new List<BreadcrumbItem>();
    protected decimal TotalAmount { get; set; } = 0;
    private bool IsLoading { get; set; } = false;

    protected override async Task OnParametersSetAsync()
    {
        _items =
          [
              new("Way Bills", href: $"/waybills"),
                    new("View Way Bill", href: null, disabled: true)
          ];
        IsLoading = true;
        await LoadWayBill();
        await LoadWayBillItems();
        TotalAmount = WayBillItems.Sum(e => e.TotalPrice);
        IsLoading = false;
    }
    
    protected async Task LoadWayBill()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/waybill/{WayBillId}");
            if (response.IsSuccessStatusCode)
            {
                var waybill = await response.Content.ReadFromJsonAsync<WayBillDto>();
                if(waybill != null)
                {
                    WayBill = waybill;
                }
                else
                {
                    SnackBar.Add("No Way Bill found!", Severity.Error);
                }
            }
            else
            {
                SnackBar.Add("Failed to load way bill", Severity.Warning);

            }
        }
        catch(Exception ex)
        {
            Logger.LogError($"Error occur loading way bill: {ex.Message}");
            SnackBar.Add("An error occured while loading waybill", Severity.Error);

        }
        
    }

    protected async Task LoadWayBillItems()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/waybill/items/{WayBillId}");
            if (response.IsSuccessStatusCode)
            {
                var wayBillItems = await response.Content.ReadFromJsonAsync<List<WayBillItemsDto>>();
                if(wayBillItems != null)
                {
                    WayBillItems = wayBillItems;
                }
                else
                {
                    SnackBar.Add("No way bill items found", Severity.Warning );
                }
            }
            else
            {
                SnackBar.Add("Failed to load way bill items ", Severity.Error );
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occur loading way bill items: {ex.Message}");
            SnackBar.Add("An error occured while loading waybill items", Severity.Error);
        }
    }


    protected async Task AddDamageItems()
    {
        var dialogParameter = new DialogParameters
        {
            {"WayBillId", WayBill.Id }
        };
        var options = new DialogOptions
        {
            CloseButton = true,
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            BackdropClick = true,
        };

        var dialog = await DialogService.ShowAsync<AddWayBillDamageItems>("Add Damage Way Bill Items",dialogParameter, options);
        var result = await dialog.Result;
    }

}
