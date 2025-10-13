using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Globalization;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImportController : ControllerBase
{
    private readonly IConfiguration _config;

    public ProductImportController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tempPath = Path.GetTempFileName();
        await using (var fs = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        var connectionString = _config.GetConnectionString("DefaultConnection");
        var ext = Path.GetExtension(file.FileName).ToLower();

        if (ext == ".xlsx")
            await ImportExcelAsync(tempPath, connectionString);
        else if (ext == ".csv")
            await ImportCsvAsync(tempPath, connectionString);
        else
            return BadRequest("Unsupported file type. Please upload .csv or .xlsx file.");

        System.IO.File.Delete(tempPath);
        return Ok("✅ Import completed successfully.");
    }

    // ======================
    // 📘 Excel Import Method
    // ======================
    private async Task ImportExcelAsync(string filePath, string connectionString)
    {
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);
        var rows = ws.RangeUsed().RowsUsed().Skip(1); // skip header

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        foreach (var row in rows)
        {
            var brandId = GetIntCell(row, 1);
            var cost = GetDecimalCell(row, 2);
            var retail = GetDecimalCell(row, 3);
            var wholesale = GetDecimalCell(row, 4);
            var size = GetDecimalCell(row, 5);
            var quantity = GetIntCell(row, 6);
            var branch = GetIntCell(row, 7);
            var measureText = row.Cell(8).GetString();
            var actualQty = GetDecimalCell(row, 9);
            var buffer = GetDecimalCell(row, 10);

            await using var cmd = new NpgsqlCommand(@"
                INSERT INTO public.""Products""
                (""BrandId"", ""CostPrice"", ""RetailPrice"", ""WholesalePrice"", ""Size"",
                 ""Quantity"", ""Branch"", ""ProductMesurementOption"", ""ActualQuantity"", ""BufferStocks"",
                 ""CreatedAt"", ""IsDeleted"", ""CreatedBy"")
                VALUES (@BrandId, @CostPrice, @RetailPrice, @WholesalePrice, @Size,
                        @Quantity, @Branch, @ProductMesurementOption, @ActualQuantity, @BufferStocks,
                        NOW(), FALSE, 'Excel Import');
            ", conn);

            cmd.Parameters.AddWithValue("@BrandId", brandId);
            cmd.Parameters.AddWithValue("@CostPrice", cost);
            cmd.Parameters.AddWithValue("@RetailPrice", retail);
            cmd.Parameters.AddWithValue("@WholesalePrice", wholesale);
            cmd.Parameters.AddWithValue("@Size", size);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            cmd.Parameters.AddWithValue("@Branch", branch);
            cmd.Parameters.AddWithValue("@ProductMesurementOption", MapMeasurement(measureText));
            cmd.Parameters.AddWithValue("@ActualQuantity", actualQty);
            cmd.Parameters.AddWithValue("@BufferStocks", buffer);

            await cmd.ExecuteNonQueryAsync();
        }
    }

    // ======================
    // 📗 CSV Import Method
    // ======================
    private async Task ImportCsvAsync(string filePath, string connectionString)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<dynamic>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        foreach (var record in records)
        {
            var d = (IDictionary<string, object>)record;

    await using var cmd = new NpgsqlCommand(@"
            INSERT INTO public.""Products""
            (""BrandId"", ""ProductName"", ""CostPrice"", ""RetailPrice"", ""WholesalePrice"", ""Size"",
             ""Quantity"", ""Branch"", ""ProductMesurementOption"", ""ActualQuantity"", ""BufferStocks"",
             ""Description"", ""Packaging"", ""CreatedAt"", ""IsDeleted"", ""CreatedBy"")
            VALUES (@BrandId, @ProductName, @CostPrice, @RetailPrice, @WholesalePrice, @Size,
                    @Quantity, @Branch, @ProductMesurementOption, @ActualQuantity, @BufferStocks,
                    @Description, @Packaging, NOW(), FALSE, 'ITAdministrator');
        ", conn);


            cmd.Parameters.AddWithValue("@BrandId", ConvertToInt(d, "BrandId"));
            cmd.Parameters.AddWithValue("@ProductName", d.TryGetValue("ProductName", out var name) ? name?.ToString() ?? "" : "");
            cmd.Parameters.AddWithValue("@Description", d.TryGetValue("Description", out var description) ? description?.ToString() ?? "" : "");
            cmd.Parameters.AddWithValue("@CostPrice", ConvertToDecimal(d, "CostPrice"));
            cmd.Parameters.AddWithValue("@RetailPrice", ConvertToDecimal(d, "RetailPrice"));
            cmd.Parameters.AddWithValue("@WholesalePrice", ConvertToDecimal(d, "WholesalePrice"));
            cmd.Parameters.AddWithValue("@Size", ConvertToDecimal(d, "Size"));
            cmd.Parameters.AddWithValue("@Quantity", ConvertToInt(d, "Quantity"));
            cmd.Parameters.AddWithValue("@Branch", ConvertToInt(d, "Branch"));
            cmd.Parameters.AddWithValue("@ProductMesurementOption", MapMeasurement(d.TryGetValue("ProductMesurementOption", out var m) ? m?.ToString() : null));
            cmd.Parameters.AddWithValue("@ActualQuantity", ConvertToDecimal(d, "ActualQuantity"));
            cmd.Parameters.AddWithValue("@BufferStocks", ConvertToDecimal(d, "BufferStocks"));
            cmd.Parameters.AddWithValue("@Packaging", d.TryGetValue("Packaging", out var p) ? p?.ToString() ?? "" : "");

            await cmd.ExecuteNonQueryAsync();

        }
    }

    // ======================
    // 🔧 Helper Methods
    // ======================
    private static int GetIntCell(IXLRangeRow row, int index)
    {
        var value = row.Cell(index).GetString();
        return int.TryParse(value, out var result) ? result : 0;
    }

    private static decimal GetDecimalCell(IXLRangeRow row, int index)
    {
        var value = row.Cell(index).GetString();
        return decimal.TryParse(value, out var result) ? result : 0;
    }

    private int ConvertToInt(IDictionary<string, object> d, string key)
    {
        return d.TryGetValue(key, out var value) && int.TryParse(value?.ToString(), out var result)
            ? result : 0;
    }

    private decimal ConvertToDecimal(IDictionary<string, object> d, string key)
    {
        return d.TryGetValue(key, out var value) && decimal.TryParse(value?.ToString(), out var result)
            ? result : 0;
    }

    private int MapMeasurement(string? option)
    {
        if (string.IsNullOrWhiteSpace(option))
            return (int)ProductMesurementOption.Piece;

        option = option.Trim().ToLower();

        return option switch
        {
            "ml" or "milliliter" => (int)ProductMesurementOption.Milliliter,
            "fl oz" or "fluidounce" => (int)ProductMesurementOption.FluidOunce,
            "ltr" or "liter" => (int)ProductMesurementOption.Liter,
            "qrt" or "quart" => (int)ProductMesurementOption.Quart,
            "pt" or "pint" => (int)ProductMesurementOption.Pint,
            "gal" or "gallon" => (int)ProductMesurementOption.Gallon,
            "pail" => (int)ProductMesurementOption.Pail,
            "box" => (int)ProductMesurementOption.Box,
            "can" => (int)ProductMesurementOption.Can,
            "bag" or "sack" => (int)ProductMesurementOption.Bag,
            "sheet" => (int)ProductMesurementOption.Sheet,
            "sachet" => (int)ProductMesurementOption.Sachet,
            "yard" => (int)ProductMesurementOption.Yard,
            "roll" => (int)ProductMesurementOption.Yard,
            "set" => (int)ProductMesurementOption.Set,
            _ => (int)ProductMesurementOption.Piece
        };
    }
}
