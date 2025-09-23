using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using static GenstarXKulayInventorySystem.Shared.Helpers.BillingHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Client.Pages.Reports;

public partial class CreateDailySaleReport
{
    [Parameter] public BranchOption Branch { get; set; }
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    [Inject] protected ILogger<CreateDailySaleReport> Logger { get; set; } = default!;
    [Inject] protected ISnackbar SnackBar { get; set; } = default!;
    [Inject] protected UserState UserState { get; set; } = default!;
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    protected DailySaleReportDto DailySaleReport { get; set; } = new();
    protected List<DailySaleDto> PaidSales { get; set; } = new();
    protected List<DailySaleDto> UnpaidSales { get; set; } = new();
    protected List<DailySaleDto> CollectedSales { get; set; } = new List<DailySaleDto>();
    protected List<BillingDto> Expenses { get; set; } = new();
    protected DateTime ReportDate { get; set; } = UtilitiesHelper.GetPhilippineTime();
    protected bool IsLoading { get; set; } = false;
    protected bool IsSaving { get; set; }  = false;
    protected bool IsValid => DailySaleReport.CashIn > 0 && DailySaleReport.BeginningBalance > 0 && !string.IsNullOrWhiteSpace(DailySaleReport.PreparedBy) ;

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        try
        {
            await LoadPaidSales();
            await LoadUnpaidSales();
            await LoadCollectedSales();
            await LoadExpenses();
            AssignFields();
            OnExpensesChange();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in OnParametersSetAsync");
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected async Task OnDateChanged(DateTime? newDate)
    {
        if (newDate == null)
            return;

        // Update ReportDate with new date (force to date only, keep time at 00:00)
        ReportDate = newDate.Value.Date;

        IsLoading = true;
        try
        {
            await LoadPaidSales();
            await LoadUnpaidSales();
            await LoadCollectedSales();
            await LoadExpenses();
            AssignFields();
            OnExpensesChange();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reloading data on date change");
            SnackBar.Add("Failed to load data for the selected date", Severity.Error);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    protected async Task LoadPaidSales()
    {
        try
        {
            
            var response = await HttpClient.GetAsync($"api/sales/all/paid/{ReportDate:yyyy-MM-dd}/{Branch}");

            if (response.IsSuccessStatusCode)
            {
                var sales = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
                if (sales != null || sales.Count > 0)
                {
                    PaidSales = sales;
                }
                else
                {
                    SnackBar.Add("No paid sales found for the selected date.", Severity.Info);
                }
            }
            else
            {
                Logger.LogError("Failed to load paid sales. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading paid sales");
        }
    }

    protected async Task LoadUnpaidSales()
    {
        try
        {

            var response = await HttpClient.GetAsync($"api/sales/all/unpaid/{ReportDate:yyyy-MM-dd}/{Branch}");

            if (response.IsSuccessStatusCode)
            {
                var sales = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
                if (sales != null || sales.Count > 0)
                {
                    UnpaidSales = sales;
                }
                else
                {
                    SnackBar.Add("No paid sales found for the selected date.", Severity.Info);
                }
            }
            else
            {
                Logger.LogError("Failed to load paid sales. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading paid sales");
        }
    }

    protected async Task LoadCollectedSales()
    {
        try
        {
            var response = await HttpClient.GetAsync($"api/sales/all/collection/{ReportDate:yyyy-MM-dd}/{Branch}");

            if (response.IsSuccessStatusCode)
            {
                var sales = await response.Content.ReadFromJsonAsync<List<DailySaleDto>>();
                if (sales != null || sales.Count > 0)
                {
                    CollectedSales = sales;
                }
                else
                {
                    SnackBar.Add("No Collected sales found for the selected date.", Severity.Info);
                }
            }
            else
            {
                Logger.LogError("Failed to load paid sales. Status Code: {StatusCode}", response.StatusCode);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex.Message, "Error loading collected sales");
        }
    }
    protected async Task LoadExpenses()
    {
        try
        {
            BillingBranch branch = Branch switch
            {
                BranchOption.GeneralSantosCity => BillingBranch.GenStar,
                BranchOption.Polomolok => BillingBranch.Kulay,
                BranchOption.Warehouse => BillingBranch.Warehouse,
                _ => BillingBranch.GenStar
            };
            var response = await HttpClient.GetAsync($"api/billings/all/expenses/{ReportDate:yyyy-MM-dd}/{branch}");

            if (response.IsSuccessStatusCode) {
                var expenses = await response.Content.ReadFromJsonAsync<List<BillingDto>>();
                Expenses = expenses ?? new List<BillingDto>();
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error loading expenses");
        }
    }

    protected void AssignFields()
    {
        DailySaleReport.InvoiceCash = PaidSales
                         .Where(ds => ds.SalesOption == PurchaseRecieptOption.BIR
                                   && ds.PaymentType != PaymentMethod.BankCheque)
                         .Sum(ds => ds.TotalAmount) ?? 0;
        DailySaleReport.InvoiceChecks = PaidSales
                         .Where(ds => ds.SalesOption == PurchaseRecieptOption.BIR
                                   && ds.PaymentType == PaymentMethod.BankCheque)
                         .Sum(ds => ds.TotalAmount) ?? 0;

        DailySaleReport.NonInvoiceCash = PaidSales.Where(ds => ds.SalesOption == PurchaseRecieptOption.NonBIR
                                   && ds.PaymentType != PaymentMethod.BankCheque)
                         .Sum(ds => ds.TotalAmount) ?? 0;

        DailySaleReport.NonInvoiceChecks = PaidSales.Where(ds => ds.SalesOption == PurchaseRecieptOption.NonBIR
                                   && ds.PaymentType == PaymentMethod.BankCheque)
                         .Sum(ds => ds.TotalAmount) ?? 0;
        DailySaleReport.CollectionCash = CollectedSales.Where(ds => ds.PaymentType != PaymentMethod.BankCheque).Sum(ds => ds.TotalAmount) ?? 0;
        DailySaleReport.CollectionChecks = CollectedSales.Where(ds => ds.PaymentType == PaymentMethod.BankCheque).Sum(ds => ds.TotalAmount) ?? 0;
        DailySaleReport.TotalCash = DailySaleReport.InvoiceCash + DailySaleReport.NonInvoiceCash;
        DailySaleReport.TotalChecks = DailySaleReport.InvoiceChecks + DailySaleReport.NonInvoiceChecks;
        DailySaleReport.ChargeSales = UnpaidSales.Sum(ds => ds.TotalAmount) ?? 0;
        DailySaleReport.TotalSales = DailySaleReport.TotalCash + DailySaleReport.TotalChecks + (DailySaleReport.ChargeSales ?? 0);
        DailySaleReport.Transportation = Expenses.Where(e => e.Category == BillingCategory.Logistics).Sum(e => e.Amount);
        DailySaleReport.Supplies = Expenses.Where(e => e.Category == BillingCategory.SchoolSupplies).Sum(e => e.Amount);
        DailySaleReport.Foods = Expenses.Where(e => e.Category == BillingCategory.Foods).Sum(e => e.Amount);
        DailySaleReport.BeginningBalance = DailySaleReport.TotalCash;
        DailySaleReport.Others = Expenses
            .Where(e => e.Category == BillingCategory.Electric || e.Category == BillingCategory.Internet || e.Category == BillingCategory.Telephone || e.Category == BillingCategory.Water || e.Category == BillingCategory.Other).Sum(e => e.Amount);
    }

    protected async Task SubmitReport()
    {   IsSaving = true;
        try
        {
            DailySaleReport.Branch = Branch;
            var response = await HttpClient.PostAsJsonAsync("api/dailysalereport", DailySaleReport);
            if (response.IsSuccessStatusCode)
            {
                SnackBar.Add("Report submitted successfully", Severity.Success);
            }
            else
            {
                SnackBar.Add("Report Already Exist", Severity.Info);
            }
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Error submitting report");
        }
        finally
        {
            await Task.Delay(1000);
            IsSaving = false;
            MudDialog.Close(DialogResult.Ok(DailySaleReport));
        }
    }

    protected void OnExpensesChange()
    {
        // Always coalesce nullables to 0
        DailySaleReport.TotalExpenses = (DailySaleReport.Transportation ?? 0)
            + (DailySaleReport.Foods ?? 0)
            + (DailySaleReport.SalaryAndAdvances ?? 0)
            + (DailySaleReport.Commissions ?? 0)
            + (DailySaleReport.Others ?? 0)
            + (DailySaleReport.Supplies ?? 0);

        DailySaleReport.TotalCashOnHand =
            (DailySaleReport.TotalSales)
            + (DailySaleReport.CollectionCash ?? 0)
            + (DailySaleReport.CollectionChecks ?? 0)
            - DailySaleReport.TotalExpenses;
        OnTotalSalesTodayChange();
        StateHasChanged();
    }

    protected void OnTotalSalesTodayChange()
    {
        DailySaleReport.TotalSalesToday = (DailySaleReport.BeginningBalance ?? 0) + (DailySaleReport.CashIn ?? 0) - DailySaleReport.TotalExpenses;
        StateHasChanged();
    }

    private void Cancel() => MudDialog.Cancel();
    private string GetBranchDisplayName(BranchOption branch)
    {
        return branch switch
        {
            BranchOption.GeneralSantosCity => "Genstar Paint Trade Center",
            BranchOption.Polomolok => "Kulay Paint Supply",
            BranchOption.Warehouse => "Warehouse",
            _ => branch.ToString()
        };
    }
}
