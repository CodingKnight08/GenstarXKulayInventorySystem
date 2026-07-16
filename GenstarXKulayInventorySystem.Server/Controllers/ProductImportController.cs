using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Globalization;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductImportController : ControllerBase
{
    private readonly IConfiguration _config;

    public ProductImportController(IConfiguration config)
    {
        _config = config;
    }
    [HttpPost("upload-brands")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var connectionString = _config.GetConnectionString("DefaultConnection");

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<dynamic>();

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        int rowNum = 2; // header row = 1

        foreach (var record in records)
        {
            try
            {
                var d = (IDictionary<string, object>)record;

                // Read Id
                if (!d.TryGetValue("Id", out var idRaw) || !int.TryParse(idRaw?.ToString(), out var brandId))
                    throw new Exception("CSV must contain a valid Id column.");

                // Read BrandName (required)
                var brandName = d.TryGetValue("BrandName", out var name) ? name?.ToString()?.Trim() : null;
                if (string.IsNullOrEmpty(brandName))
                    throw new Exception("BrandName is required.");

                // Read Description (optional)
                var description = d.TryGetValue("Description", out var desc) ? desc?.ToString()?.Trim() : "";

                // Insert ProductBrand using ID from CSV
                await using var cmd = new SqlCommand(@"
                    SET IDENTITY_INSERT ProductBrands ON;

                    INSERT INTO ProductBrands
                    (Id, BrandName, Description, CreatedAt, CreatedBy, IsDeleted)
                    VALUES (@Id, @BrandName, @Description, GETDATE(), 'ITAdministrator', 0);

                    SET IDENTITY_INSERT ProductBrands OFF;
                ", conn);

                cmd.Parameters.AddWithValue("@Id", brandId);
                cmd.Parameters.AddWithValue("@BrandName", brandName);
                cmd.Parameters.AddWithValue("@Description", description ?? "");

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error in CSV row {rowNum}: {ex.Message}");
            }

            rowNum++;
        }

        return Ok("Product brands imported successfully.");
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

        try
        {
            if (ext == ".xlsx")
                await ImportExcelAsync(tempPath, connectionString);
            else if (ext == ".csv")
                await ImportCsvAsync(tempPath, connectionString);
            else
                return BadRequest("Unsupported file type. Please upload .csv or .xlsx file.");
        }
        catch (Exception ex)
        {
            // Catch any exception and return detailed info
            return StatusCode(500, $"Error during import: {ex.Message} \n {ex.StackTrace}");
        }
        finally
        {
            System.IO.File.Delete(tempPath);
        }

        return Ok("✅ Import completed successfully.");
    }
    [HttpPost("update-global-products")]
    public async Task<IActionResult> UpdateGlobalProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var connectionString = _config.GetConnectionString("DefaultConnection");

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<dynamic>();

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        int rowNum = 2;
        int updatedCount = 0;

        foreach (var record in records)
        {
            try
            {
                var d = (IDictionary<string, object>)record;

                // Read Id
                if (!d.TryGetValue("Id", out var idRaw) ||
                    !int.TryParse(idRaw?.ToString(), out var productId))
                {
                    throw new Exception("CSV must contain a valid Id column.");
                }

                // Read ProductName
                var productName = d.TryGetValue("ProductName", out var name)
                    ? name?.ToString()?.Trim()
                    : null;

                if (string.IsNullOrWhiteSpace(productName))
                {
                    throw new Exception("ProductName is required.");
                }

                await using var cmd = new SqlCommand(@"
                UPDATE GlobalProducts
                SET ProductName = @ProductName
                WHERE Id = @Id
            ", conn);

                cmd.Parameters.AddWithValue("@Id", productId);
                cmd.Parameters.AddWithValue("@ProductName", productName);

                updatedCount += await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error in CSV row {rowNum}: {ex.Message}");
            }

            rowNum++;
        }

        return Ok($"{updatedCount} GlobalProducts updated successfully.");
    }
    // ======================
    // 📘 Excel Import Method
    // ======================
    private async Task ImportExcelAsync(string filePath, string connectionString)
    {
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);
        var rows = ws.RangeUsed().RowsUsed().Skip(1); // skip header

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        foreach (var row in rows)
        {
            try
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

                // ✅ Check if BrandId exists
                await using var checkCmd = new SqlCommand("SELECT COUNT(*) FROM ProductBrands WHERE Id=@BrandId", conn);
                checkCmd.Parameters.AddWithValue("@BrandId", brandId);
                var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                if (!exists)
                {
                    throw new Exception($"BrandId {brandId} does not exist in ProductBrands. Row: {row.RowNumber()}");
                }

                await using var cmd = new SqlCommand(@"
                    INSERT INTO Products
                    (BrandId, CostPrice, RetailPrice, WholesalePrice, Size,
                     Quantity, Branch, ProductMesurementOption, ActualQuantity, BufferStocks,
                     CreatedAt, IsDeleted, CreatedBy)
                    VALUES (@BrandId, @CostPrice, @RetailPrice, @WholeSalePrice, @Size,
                            @Quantity, @Branch, @ProductMesurementOption, @ActualQuantity, @BufferStocks,
                            GETDATE(), 0, 'ITAdministrator');
                ", conn);

                cmd.Parameters.AddWithValue("@BrandId", brandId);
                cmd.Parameters.AddWithValue("@CostPrice", cost);
                cmd.Parameters.AddWithValue("@RetailPrice", retail);
                cmd.Parameters.AddWithValue("@WholeSalePrice", wholesale);
                cmd.Parameters.AddWithValue("@Size", size);
                cmd.Parameters.AddWithValue("@Quantity", quantity);
                cmd.Parameters.AddWithValue("@Branch", branch);
                cmd.Parameters.AddWithValue("@ProductMesurementOption", MapMeasurement(measureText));
                cmd.Parameters.AddWithValue("@ActualQuantity", actualQty);
                cmd.Parameters.AddWithValue("@BufferStocks", buffer);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in Excel row {row.RowNumber()}: {ex.Message}");
            }
        }
    }

 
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

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        int rowNum = 2;

        foreach (var record in records)
        {
            try
            {
                var d = (IDictionary<string, object>)record;

                // ---------------------------------------------------
                // 1) Read MasterProductId from CSV column "Id"
                // ---------------------------------------------------
                if (!d.TryGetValue("Id", out var idRaw) ||
                    !int.TryParse(idRaw?.ToString(), out var masterProductId))
                {
                    throw new Exception("CSV must contain a valid Id column for MasterProductId.");
                }

                // ---------------------------------------------------
                // 2) VALIDATE that MasterProductId EXISTS in GlobalProducts
                // ---------------------------------------------------
                await using (var checkMaster = new SqlCommand(
                    "SELECT COUNT(*) FROM GlobalProducts WHERE Id = @Id",
                    conn))
                {
                    checkMaster.Parameters.AddWithValue("@Id", masterProductId);

                    if ((int)await checkMaster.ExecuteScalarAsync() == 0)
                        throw new Exception($"MasterProductId {masterProductId} does not exist in GlobalProducts.");
                }

                // ---------------------------------------------------
                // 3) Optional validation for BrandId (if CSV includes it)
                // ---------------------------------------------------
                int? brandId = null;
                if (d.TryGetValue("BrandId", out var b) &&
                    int.TryParse(b?.ToString(), out var parsedBrand))
                {
                    brandId = parsedBrand;

                    await using var checkBrand = new SqlCommand(
                        "SELECT COUNT(*) FROM ProductBrands WHERE Id = @BrandId",
                        conn
                    );
                    checkBrand.Parameters.AddWithValue("@BrandId", brandId.Value);

                    if ((int)await checkBrand.ExecuteScalarAsync() == 0)
                        throw new Exception($"BrandId {brandId} does not exist.");
                }

                // ---------------------------------------------------
                // 4) Insert BranchProduct RECORD ONLY
                // ---------------------------------------------------
                await using var insertBranch = new SqlCommand(@"
                INSERT INTO BranchProducts
                (MasterProductId, Branch, CostPrice, RetailPrice, WholeSalePrice, Size,
                 ActualQuantity, BufferStocks, ProductMesurementOption,
                 CreatedAt, CreatedBy, IsDeleted)
                VALUES
                (@MasterProductId, @Branch, @CostPrice, @RetailPrice, @WholeSalePrice, @Size,
                 @ActualQuantity, @BufferStocks, @ProductMesurementOption,
                 GETDATE(), 'ITAdministrator', 0);
            ", conn);

                // MasterProductId from CSV
                insertBranch.Parameters.AddWithValue("@MasterProductId", masterProductId);

                // BranchOption column
                insertBranch.Parameters.AddWithValue("@Branch",
                    d.TryGetValue("Branch", out var branch) &&
                    int.TryParse(branch?.ToString(), out var branchVal)
                        ? branchVal
                        : 0
                );

                // Numeric columns
                insertBranch.Parameters.AddWithValue("@CostPrice", ConvertToDecimal(d, "CostPrice"));
                insertBranch.Parameters.AddWithValue("@RetailPrice", ConvertToDecimal(d, "RetailPrice"));
                insertBranch.Parameters.AddWithValue("@WholeSalePrice", ConvertToDecimal(d, "WholeSalePrice"));
                insertBranch.Parameters.AddWithValue("@Size", ConvertToDecimal(d, "Size"));
                insertBranch.Parameters.AddWithValue("@ActualQuantity", ConvertToDecimal(d, "ActualQuantity"));
                insertBranch.Parameters.AddWithValue("@BufferStocks", ConvertToDecimal(d, "BufferStocks"));

                // Measurement option
                int measurement = 0;
                if (d.TryGetValue("ProductMesurementOption", out var m) &&
                    int.TryParse(m?.ToString(), out var parsed))
                {
                    measurement = parsed;
                }
                insertBranch.Parameters.AddWithValue("@ProductMesurementOption", measurement);

                await insertBranch.ExecuteNonQueryAsync();

            }
            catch (Exception ex)
            {
                throw new Exception($"Error in CSV row {rowNum}: {ex.Message}");
            }

            rowNum++;
        }
    }



    [HttpPost("upload-globalproducts")]
    public async Task<IActionResult> UploadGlobalProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLower();
        var tempPath = Path.GetTempFileName();

        await using (var fs = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        try
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            if (ext == ".xlsx")
                await ImportGlobalProductsExcelAsync(tempPath, connectionString);
            else if (ext == ".csv")
                await ImportGlobalProductsCsvAsync(tempPath, connectionString);
            else
                return BadRequest("Only .xlsx or .csv files are supported.");

            return Ok("✅ GlobalProducts import completed successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ Import failed: {ex.Message}");
        }
        finally
        {
            System.IO.File.Delete(tempPath);
        }
    }

    private async Task ImportGlobalProductsExcelAsync(
    string filePath,
    string connectionString)
    {
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);
        var rows = ws.RangeUsed().RowsUsed().Skip(1);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        foreach (var row in rows)
        {
            var id = GetIntCell(row, 1);
            var brandId = GetIntCell(row, 2);
            var productName = row.Cell(3).GetString();
            var description = row.Cell(4).GetString();

            if (string.IsNullOrWhiteSpace(productName))
                throw new Exception($"Row {row.RowNumber()}: ProductName is required.");

            bool exists = false;
            if (id > 0)
            {
                await using var check = new SqlCommand(
                    "SELECT COUNT(*) FROM GlobalProducts WHERE Id = @Id", conn);
                check.Parameters.AddWithValue("@Id", id);
                exists = (int)await check.ExecuteScalarAsync() > 0;
            }

            await UpsertGlobalProduct(conn, id, brandId, productName, description, row.RowNumber());

        }
    }

    private async Task ImportGlobalProductsCsvAsync(
    string filePath,
    string connectionString)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<dynamic>();

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        int rowNum = 2;

        foreach (var record in records)
        {
            var d = (IDictionary<string, object>)record;

            int id = d.TryGetValue("Id", out var v) && int.TryParse(v?.ToString(), out var parsed)
                ? parsed
                : 0;

            int brandId = int.Parse(d["BrandId"].ToString());
            string productName = d["ProductName"]?.ToString() ?? "";
            string description = d.TryGetValue("Description", out var desc)
                ? desc?.ToString()
                : "";

            if (string.IsNullOrWhiteSpace(productName))
                throw new Exception($"Row {rowNum}: ProductName is required.");

            bool exists = false;
            if (id > 0)
            {
                await using var check = new SqlCommand(
                    "SELECT COUNT(*) FROM GlobalProducts WHERE Id = @Id", conn);
                check.Parameters.AddWithValue("@Id", id);
                exists = (int)await check.ExecuteScalarAsync() > 0;
            }

            await UpsertGlobalProduct(conn, id, brandId, productName, description, rowNum);


            rowNum++;
        }
    }
    private static async Task UpsertGlobalProduct(
    SqlConnection conn,
    int id,
    int brandId,
    string productName,
    string description,
    int rowNumber = 0)  // optional for error messages
    {
        // -------------------- 1️⃣ Validate BrandId --------------------
        await using var checkBrand = new SqlCommand(
            "SELECT COUNT(*) FROM ProductBrands WHERE Id = @BrandId", conn);
        checkBrand.Parameters.AddWithValue("@BrandId", brandId);
        bool brandExists = (int)await checkBrand.ExecuteScalarAsync() > 0;

        if (!brandExists)
        {
            throw new Exception(rowNumber > 0
                ? $"Row {rowNumber}: BrandId {brandId} does not exist in ProductBrands."
                : $"BrandId {brandId} does not exist in ProductBrands.");
        }

        // -------------------- 2️⃣ Check if product exists --------------------
        bool exists = false;
        if (id > 0)
        {
            await using var checkProduct = new SqlCommand(
                "SELECT COUNT(*) FROM GlobalProducts WHERE Id = @Id", conn);
            checkProduct.Parameters.AddWithValue("@Id", id);
            exists = (int)await checkProduct.ExecuteScalarAsync() > 0;
        }

        // -------------------- 3️⃣ Update existing product --------------------
        if (exists)
        {
            await using var updateCmd = new SqlCommand(@"
            UPDATE GlobalProducts
            SET BrandId = @BrandId,
                ProductName = @ProductName,
                Description = @Description,
                UpdatedAt = GETDATE(),
                UpdatedBy = 'Import'
            WHERE Id = @Id
        ", conn);

            updateCmd.Parameters.AddWithValue("@Id", id);
            updateCmd.Parameters.AddWithValue("@BrandId", brandId);
            updateCmd.Parameters.AddWithValue("@ProductName", productName.Trim());
            updateCmd.Parameters.AddWithValue("@Description", description ?? "");

            await updateCmd.ExecuteNonQueryAsync();
        }
        // -------------------- 4️⃣ Insert new product --------------------
        else
        {
            await using var insertCmd = new SqlCommand(@"
            INSERT INTO GlobalProducts
            (BrandId, ProductName, Description, Packaging, CreatedAt, CreatedBy, IsDeleted)
            VALUES
            (@BrandId, @ProductName, @Description, @Packaging, GETDATE(), 'ITAdministrator  ', 0)
        ", conn);

            insertCmd.Parameters.AddWithValue("@BrandId", brandId);
            insertCmd.Parameters.AddWithValue("@ProductName", productName.Trim());
            insertCmd.Parameters.AddWithValue("@Description", description ?? "");
            insertCmd.Parameters.AddWithValue("@Packaging", "");

            await insertCmd.ExecuteNonQueryAsync();
        }
    }

    ///-----------------------------------------------------------------------
    //BranchProduct add 

    [HttpPost("upload-branchproducts")]
    public async Task<IActionResult> UploadBranchProducts(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tempPath = Path.GetTempFileName();
        await using (var fs = new FileStream(tempPath, FileMode.Create))
            await file.CopyToAsync(fs);

        var connectionString = _config.GetConnectionString("DefaultConnection");
        var ext = Path.GetExtension(file.FileName).ToLower();

        try
        {
            if (ext == ".xlsx")
                await ImportBranchProductsExcelAsync(tempPath, connectionString);
            else if (ext == ".csv")
                await ImportBranchProductsCsvAsync(tempPath, connectionString);
            else
                return BadRequest("Unsupported file type. Please upload .csv or .xlsx file.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error during import: {ex.Message}");
        }
        finally
        {
            System.IO.File.Delete(tempPath);
        }

        return Ok("✅ Branch products imported successfully.");
    }

    private async Task ImportBranchProductsCsvAsync(string filePath, string connectionString)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<dynamic>();

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        int rowNum = 2;
        foreach (var record in records)
        {
            var d = (IDictionary<string, object>)record;

            int masterProductId = ParseInt(d, "MasterProductId");
            if (masterProductId == 0)
                throw new Exception($"Row {rowNum}: MasterProductId is required and must exist.");

            // Check MasterProduct exists
            await using var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM GlobalProducts WHERE Id=@Id", conn);
            checkCmd.Parameters.AddWithValue("@Id", masterProductId);
            if ((int)await checkCmd.ExecuteScalarAsync() == 0)
                throw new Exception($"Row {rowNum}: MasterProductId {masterProductId} does not exist.");

            // Branch
            int branchInt = ParseInt(d, "Branch");
            BranchOption branch = branchInt switch
            {
                1 => BranchOption.GeneralSantosCity,
                2 => BranchOption.Warehouse,
                _ => BranchOption.Polomolok
            };

            // Numeric fields
            decimal costPrice = ParseDecimal(d, "CostPrice");
            decimal retailPrice = ParseDecimal(d, "RetailPrice");
            decimal? wholesalePrice = ParseNullableDecimal(d, "WholeSalePrice");
            decimal size = ParseDecimal(d, "Size");
            decimal actualQuantity = ParseDecimal(d, "ActualQuantity");
            decimal bufferStocks = ParseDecimal(d, "BufferStocks");
            int productMesurementOption = ParseInt(d, "ProductMesurementOption");
            //string measurementStr = d.ContainsKey("ProductMesurement") ? (d["ProductMesurement"]?.ToString() ?? "") : "";
            //int productMesurementOption = MapMeasurement(measurementStr);

            // Insert
            await using var cmd = new SqlCommand(@"
            INSERT INTO BranchProducts
            (MasterProductId, Branch, CostPrice, RetailPrice, WholeSalePrice, Size,
             ActualQuantity, BufferStocks, ProductMesurementOption,
             CreatedAt, CreatedBy, IsDeleted)
            VALUES
            (@MasterProductId, @Branch, @CostPrice, @RetailPrice, @WholeSalePrice, @Size,
             @ActualQuantity, @BufferStocks, @ProductMesurementOption,
             GETDATE(), 'ITAdministrator', 0)
        ", conn);

            cmd.Parameters.AddWithValue("@MasterProductId", masterProductId);
            cmd.Parameters.AddWithValue("@Branch", (int)branch);
            cmd.Parameters.AddWithValue("@CostPrice", costPrice);
            cmd.Parameters.AddWithValue("@RetailPrice", retailPrice);
            cmd.Parameters.AddWithValue("@WholeSalePrice", (object?)wholesalePrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Size", size);
            cmd.Parameters.AddWithValue("@ActualQuantity", actualQuantity);
            cmd.Parameters.AddWithValue("@BufferStocks", bufferStocks);
            cmd.Parameters.AddWithValue("@ProductMesurementOption", productMesurementOption);

            await cmd.ExecuteNonQueryAsync();
            rowNum++;
        }
    }

    private async Task ImportBranchProductsExcelAsync(string filePath, string connectionString)
    {
        using var workbook = new XLWorkbook(filePath);
        var ws = workbook.Worksheet(1);
        var rows = ws.RangeUsed().RowsUsed().Skip(1);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        foreach (var row in rows)
        {
            int masterProductId = GetIntCell(row, 1);
            if (masterProductId == 0)
                throw new Exception($"Row {row.RowNumber()}: MasterProductId is required.");

            // Validate MasterProduct exists
            await using var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM GlobalProducts WHERE Id=@Id", conn);
            checkCmd.Parameters.AddWithValue("@Id", masterProductId);
            if ((int)await checkCmd.ExecuteScalarAsync() == 0)
                throw new Exception($"Row {row.RowNumber()}: MasterProductId {masterProductId} does not exist.");

            int branchInt = GetIntCell(row, 2);
            BranchOption branch = branchInt switch
            {
                1 => BranchOption.GeneralSantosCity,
                2 => BranchOption.Warehouse,
                _ => BranchOption.Polomolok
            };

            decimal costPrice = GetDecimalCell(row, 3);
            decimal retailPrice = GetDecimalCell(row, 4);
            decimal? wholesalePrice = GetNullableDecimalCell(row, 5);
            decimal size = GetDecimalCell(row, 6);
            decimal actualQuantity = GetDecimalCell(row, 7);
            decimal bufferStocks = GetDecimalCell(row, 8);

            int productMesurementOption = MapMeasurement(row.Cell(9).GetString());

            await using var cmd = new SqlCommand(@"
            INSERT INTO BranchProducts
            (MasterProductId, Branch, CostPrice, RetailPrice, WholeSalePrice, Size,
             ActualQuantity, BufferStocks, ProductMesurementOption,
             CreatedAt, CreatedBy, IsDeleted)
            VALUES
            (@MasterProductId, @Branch, @CostPrice, @RetailPrice, @WholeSalePrice, @Size,
             @ActualQuantity, @BufferStocks, @ProductMesurementOption,
             GETDATE(), 'ITAdministrator', 0)
        ", conn);

            cmd.Parameters.AddWithValue("@MasterProductId", masterProductId);
            cmd.Parameters.AddWithValue("@Branch", (int)branch);
            cmd.Parameters.AddWithValue("@CostPrice", costPrice);
            cmd.Parameters.AddWithValue("@RetailPrice", retailPrice);
            cmd.Parameters.AddWithValue("@WholeSalePrice", wholesalePrice);
            cmd.Parameters.AddWithValue("@Size", size);
            cmd.Parameters.AddWithValue("@ActualQuantity", actualQuantity);
            cmd.Parameters.AddWithValue("@BufferStocks", bufferStocks);
            cmd.Parameters.AddWithValue("@ProductMesurementOption", productMesurementOption);

            await cmd.ExecuteNonQueryAsync();
        }
    }


    [HttpPost("update-branchproducts-actualquantity")]
    public async Task<IActionResult> UpdateBranchProductsActualQuantity(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var tempPath = Path.GetTempFileName();
        await using (var fs = new FileStream(tempPath, FileMode.Create))
            await file.CopyToAsync(fs);

        try
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            await UpdateBranchProductsActualQuantityCsvAsync(tempPath, connectionString);

            return Ok("✅ BranchProducts ActualQuantity updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"❌ Update failed: {ex.Message}");
        }
        finally
        {
            System.IO.File.Delete(tempPath);
        }
    }

    private async Task UpdateBranchProductsActualQuantityCsvAsync(
    string filePath,
    string connectionString)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<dynamic>();

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        int rowNum = 2;

        foreach (var record in records)
        {
            var d = (IDictionary<string, object>)record;

            // 1️⃣ BranchProduct Id (REQUIRED)
            if (!d.TryGetValue("Id", out var idRaw) ||
                !int.TryParse(idRaw?.ToString(), out var branchProductId))
            {
                throw new Exception($"Row {rowNum}: Id is required and must be numeric.");
            }

            // 2️⃣ ActualQuantity (REQUIRED)
            if (!d.TryGetValue("ActualQuantity", out var qtyRaw) ||
                !decimal.TryParse(qtyRaw?.ToString(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var actualQty))
            {
                throw new Exception($"Row {rowNum}: ActualQuantity is required and must be numeric.");
            }

            // 3️⃣ Ensure BranchProduct exists
            await using (var checkCmd = new SqlCommand(
                "SELECT COUNT(*) FROM BranchProducts WHERE Id = @Id", conn))
            {
                checkCmd.Parameters.AddWithValue("@Id", branchProductId);

                if ((int)await checkCmd.ExecuteScalarAsync() == 0)
                    throw new Exception($"Row {rowNum}: BranchProduct Id {branchProductId} does not exist.");
            }

            // 4️⃣ UPDATE ONLY ActualQuantity
            await using var updateCmd = new SqlCommand(@"
            UPDATE BranchProducts
            SET ActualQuantity = @ActualQuantity,
                UpdatedAt = GETDATE(),
                UpdatedBy = 'CSV-IMPORT'
            WHERE Id = @Id
        ", conn);

            updateCmd.Parameters.AddWithValue("@Id", branchProductId);
            updateCmd.Parameters.AddWithValue("@ActualQuantity", actualQty);

            await updateCmd.ExecuteNonQueryAsync();

            rowNum++;
        }
    }


    private int ParseInt(IDictionary<string, object> d, string key)
    {
        return d.TryGetValue(key, out var v) && int.TryParse(v?.ToString(), out var val) ? val : 0;
    }

    private decimal ParseDecimal(IDictionary<string, object> d, string key)
    {
        // Use InvariantCulture to correctly parse numbers like "12.50"
        if (d.TryGetValue(key, out var v) &&
            decimal.TryParse(v?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
            return val;

        return 0;
    }
    private decimal? ParseNullableDecimal(IDictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var v))
        {
            var str = v?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(str) &&
                decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var val))
                return val;
        }
        return null; // return null if empty or invalid
    }


    private static decimal? GetNullableDecimalCell(IXLRangeRow row, int index)
    {
        var value = row.Cell(index).GetString().Trim();
        if (string.IsNullOrEmpty(value)) return null;

        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
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
        var value = row.Cell(index).GetString().Trim();
        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
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

        option = option.Trim().ToLowerInvariant();

        return option switch
        {
            "ml" or "milliliter" => (int)ProductMesurementOption.Milliliter,
            "fl oz" or "fluid ounce" => (int)ProductMesurementOption.FluidOunce,
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
            "roll" => (int)ProductMesurementOption.Roll, // fix mapping
            "set" => (int)ProductMesurementOption.Set,
            _ => (int)ProductMesurementOption.Piece
        };
    }




}
