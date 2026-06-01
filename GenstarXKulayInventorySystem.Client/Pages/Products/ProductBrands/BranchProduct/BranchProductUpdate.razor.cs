using ClosedXML.Excel;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

namespace GenstarXKulayInventorySystem.Client.Pages.Products.ProductBrands.BranchProduct;

public partial class BranchProductUpdate
{
    [Inject] protected HttpClient HttpClient { get; set; } = default!;
    protected List<BranchProductPrice> ToBeupdated { get; set; } = new List<BranchProductPrice>();

    protected string Message = "";
    protected bool IsError = false;

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        try
        {
            var file = e.File;

            if (file == null)
            {
                Message = "❌ No file selected";
                IsError = true;
                return;
            }

            await using var uploadStream = file.OpenReadStream(20 * 1024 * 1024);

            // ✅ IMPORTANT FIX: copy to memory stream
            using var memoryStream = new MemoryStream();
            await uploadStream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);

            var ws = workbook.Worksheet(1);
            var rows = ws.RangeUsed().RowsUsed().Skip(1);

            ToBeupdated = new List<BranchProductPrice>();

            foreach (var row in rows)
            {
                ToBeupdated.Add(new BranchProductPrice
                {
                    BranchProductId = GetInt(row, 1),
                    BrandName = row.Cell(2).GetString(),
                    ProductName = row.Cell(3).GetString(),
                    CostPrice = GetDecimal(row, 4),
                    RetailPrice = GetDecimal(row, 5),
                    WholeSalePrice = GetDecimal(row, 6)
                });
            }

            Message = $"✅ Loaded {ToBeupdated.Count} records";
            IsError = false;
        }
        catch (Exception ex)
        {
            Message = $"❌ Error: {ex.Message}";
            IsError = true;
        }
    }
    // ---------------- Helpers ----------------
    private int GetInt(IXLRangeRow row, int index)
    {
        return int.TryParse(row.Cell(index).GetString(), out var val) ? val : 0;
    }

    private decimal? GetDecimal(IXLRangeRow row, int index)
    {
        var val = row.Cell(index).GetString();
        return decimal.TryParse(val, out var result) ? result : null;
    }

    protected async Task UpdatePrices()
    {
        if (ToBeupdated == null || !ToBeupdated.Any())
        {
            Message = "❌ No data to update";
            IsError = true;
            return;
        }
        try
        {
            var response = await HttpClient.PutAsJsonAsync("api/branchproduct/update-prices", ToBeupdated);
            if (response.IsSuccessStatusCode)
            {
                var resultMessage = await response.Content.ReadAsStringAsync();
                Message = $"✅ Success: {resultMessage}";
                IsError = false;
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Message = $"❌ Failed: {errorMessage}";
                IsError = true;
            }
        }
        catch (Exception ex)
        {
            Message = $"❌ Error: {ex.Message}";
            IsError = true;
        }
    }
}
