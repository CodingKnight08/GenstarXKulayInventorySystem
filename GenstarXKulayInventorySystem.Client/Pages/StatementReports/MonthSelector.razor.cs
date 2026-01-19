using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.StatementReports;

public partial class MonthSelector
{
    [Parameter] public int ClientId { get; set; }
    [CascadingParameter] protected IMudDialogInstance MudDialog { get; set; } = default!;
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private ILogger<MonthSelector> Logger { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    protected StatementOfAccountDataDto Data { get; set; } = new StatementOfAccountDataDto();
    protected int SelectedMonth { get; set; } = DateTime.Now.Month;
    protected int SelectedYear { get; set; } = DateTime.Now.Year;
    protected bool IsLoading { get; set; } = false;
    protected string? PdfUrl { get; set; }
    protected List<(int Value, string Text)> AllowedMonths { get; set; } = new();
    protected List<int> AllowedYears { get; set; } = new();

    protected override void OnParametersSet()
    {
        AllowedMonths = Enumerable.Range(1, 12)
            .Select(m => (Value: m, Text: System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m)))
            .ToList();
        int currentYear = DateTime.Now.Year;
        AllowedYears = Enumerable.Range(currentYear - 5, 6).ToList();
    }
    protected async Task OnYearChanged(int value)
    {
        SelectedYear = value;
        await LoadData();
    }
    protected async Task OnMonthChanged(int value)
    {
        SelectedMonth = value;
        await LoadData();
        
    }
    protected async Task LoadData()
    {
        IsLoading = true;
        try
        {
            var response = await HttpClient.GetAsync($"api/statementreport/monthly-charges/{ClientId}/{SelectedMonth}/{SelectedYear}");
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "Failed to load statement of account. StatusCode: {StatusCode}",
                    response.StatusCode);
                return;
            }

            Data = await response.Content
                .ReadFromJsonAsync<StatementOfAccountDataDto>()
                ?? new StatementOfAccountDataDto();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading statement of account data for client {ClientId} and month {Month}", ClientId, SelectedMonth,SelectedYear);
        }
        finally { 
        IsLoading = false;
        }
        StateHasChanged();
    }


    private async Task GeneratePdf()
    {
        if (Data is null)
            return;

        var response = await HttpClient.PostAsJsonAsync(
            "api/statementreport/generate",
            Data);

        if (!response.IsSuccessStatusCode)
            return;

        var pdfBytes = await response.Content.ReadAsByteArrayAsync();

        await DownloadPdf(
            pdfBytes,
            $"Statement_{Data.ClientName}_{DateTime.Now:yyyyMMdd}.pdf");
    }
    private async Task DownloadPdf(byte[] pdfBytes, string fileName)
    {
        using var streamRef = new DotNetStreamReference(stream: new MemoryStream(pdfBytes));
        await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    protected async Task Submit()
    {
        await GeneratePdf();
        MudDialog.Close(DialogResult.Ok(true));
    }
    protected void Cancel()
    {
        MudDialog.Cancel();
    }
}
