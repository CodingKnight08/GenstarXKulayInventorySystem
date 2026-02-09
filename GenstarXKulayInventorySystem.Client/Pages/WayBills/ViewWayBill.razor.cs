using GenstarXKulayInventorySystem.Client.Pages.WayBills.WayBillDamageItems;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.WalBillHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.WayBills;

public partial class ViewWayBill
{
    [Parameter] public int WayBillId { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<ViewWayBill> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected WayBillDto WayBill { get; set; } = new WayBillDto();
    protected List<WayBillItemsDto> WayBillItems { get; set; } = new List<WayBillItemsDto>();
    protected List<WayBillDamageItemDto> DamageItems { get; set; } = new List<WayBillDamageItemDto>();
    protected List<BreadcrumbItem> _items { get; set; } = new List<BreadcrumbItem>();
    protected decimal TotalAmount { get; set; } = 0;
    protected decimal TotalDamageAmount { get; set; } = 0;
    private bool IsLoading { get; set; } = false;
    private bool HasItems { get; set; } = false;
    private bool HasDamageItems { get; set; } = false;
    private bool IsSync { get; set; } = false;
    private bool IsEdit { get; set; } = false;
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
        await LoadDamageItems();
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
                    HasItems = true;
                    IsSync = WayBillItems.Any() && WayBillItems.All(e => e.IsMergeToSystem);

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
        if (result.Canceled)
            return;
        IsLoading = true;
        await LoadWayBill();
        await LoadWayBillItems();
        await LoadDamageItems();
        TotalAmount = WayBillItems.Sum(e => e.TotalPrice);
        IsLoading = false;
        StateHasChanged();
    }
    protected async Task LoadDamageItems()
    {
        try
        {


            var response = await HttpClient.GetAsync($"api/waybill/all/damage/{WayBillId}");
            if (response.IsSuccessStatusCode)
            {
                var damages = await response.Content.ReadFromJsonAsync<List<WayBillDamageItemDto>>();
                if (damages is not null && damages.Count > 0)
                {
                    DamageItems = damages;
                    TotalDamageAmount = DamageItems.Sum(e => e.TotalDamageCost);
                    HasDamageItems = true;
                }
                else
                {
                    HasDamageItems = false;
                }
            }
            else
            {
                Logger.LogInformation("StatusCode unsuccessful");
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error occured upon retrieving damage items ");
        }
    }


    protected async Task UpdateWayBillAsync()
    {
        try
        {
            var response = await HttpClient.PutAsJsonAsync(
                $"api/waybill/{WayBill.Id}",
                WayBill);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("WayBill updated successfully", Severity.Success);
                await LoadWayBill();
                StateHasChanged();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                SnackBar.Add($"Update failed: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating WayBill");
            SnackBar.Add("Unexpected error while updating WayBill", Severity.Error);
        }
        finally
        {
            IsEdit = false;
        }
    }

    protected async Task SyncWayBillItemsAsync()
    {
        if (WayBillItems == null || !WayBillItems.Any())
        {
            SnackBar.Add("No waybill items to sync.", Severity.Warning);
            return;
        }

        // Optional: prevent double sync
        if (IsSync)
        {
            SnackBar.Add("WayBill items are already synced.", Severity.Info);
            return;
        }

        IsLoading = true;

        try
        {
            var response = await HttpClient.PutAsJsonAsync(
                "api/waybill/sync-branch-products",
                WayBillItems);

            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("WayBill items synced successfully.", Severity.Success);

                // Reload items so IsMergeToSystem updates
                await LoadWayBillItems();

                IsSync = WayBillItems.Any() &&
                         WayBillItems.All(x => x.IsMergeToSystem);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                SnackBar.Add($"Sync failed: {error}", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error syncing WayBill items");
            SnackBar.Add("Unexpected error during sync.", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnDateReceiveChange(DateTime? date)
    {
        WayBill.DateReceived = date ?? PhilippineTime.Now;
        OnTermsChange(WayBill.WayBillTerms);
         await Task.CompletedTask;
    }
    private void OnTermsChange(WayBillTermsOption terms)
    {
        WayBill.WayBillTerms = terms;


        WayBill.ExpectedPaymentDate = terms switch
        {
            WayBillTermsOption.Day7 => WayBill.DateReceived.AddDays(7),
            WayBillTermsOption.Day15 => WayBill.DateReceived.AddDays(15),
            WayBillTermsOption.Day30 => WayBill.DateReceived.AddDays(30),
            WayBillTermsOption.Day60 => WayBill.DateReceived.AddDays(60),
            WayBillTermsOption.Day90 => WayBill.DateReceived.AddDays(90),
            WayBillTermsOption.Day120 => WayBill.DateReceived.AddDays(120),
            _ => WayBill.ExpectedPaymentDate
        };
    }


    protected async Task CancelUpdate()
    {
        IsEdit = false;
        await LoadWayBill();
        StateHasChanged();
    }
}
