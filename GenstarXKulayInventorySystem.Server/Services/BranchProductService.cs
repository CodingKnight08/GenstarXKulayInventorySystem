using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace GenstarXKulayInventorySystem.Server.Services;

public class BranchProductService: IBranchProductService
{
    private readonly InventoryDbContext _context;
    private readonly ILogger<BranchProductService> _logger;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _connectionString;
    public BranchProductService(IConfiguration config, InventoryDbContext context, ILogger<BranchProductService> logger, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
         _httpContextAccessor = httpContextAccessor;
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }

    public async Task<bool> UpdateBranchProductPrices(List<BranchProductPrice> items)
    {
        try
        {
            if (items == null || !items.Any())
                return false;

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            foreach (var item in items)
            {
                await using var cmd = new SqlCommand(@"
                UPDATE BranchProducts
                SET CostPrice = @CostPrice,
                    RetailPrice = @RetailPrice,
                    WholeSalePrice = @WholeSalePrice,
                    UpdatedAt = GETDATE(),
                    UpdatedBy = 'EXCEL-IMPORT'
                WHERE Id = @Id
            ", conn);

                cmd.Parameters.AddWithValue("@Id", item.BranchProductId);
                cmd.Parameters.AddWithValue("@CostPrice", (object?)item.CostPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RetailPrice", (object?)item.RetailPrice ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WholeSalePrice", (object?)item.WholeSalePrice ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating branch product prices");
            return false;
        }
    }


}
public interface IBranchProductService
{
    Task<bool> UpdateBranchProductPrices(List<BranchProductPrice> items);
}