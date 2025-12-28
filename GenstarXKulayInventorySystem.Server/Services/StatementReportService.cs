using AutoMapper;
using GenstarXKulayInventorySystem.Server.Reports;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Services;

public class StatementReportService:IStatementReportService
{
    private readonly InventoryDbContext _dbContext;
    private readonly ILogger<StatementReportService> _logger;
    private readonly IMapper _mapper;

    public StatementReportService(InventoryDbContext dbContext, ILogger<StatementReportService> logger, IMapper mapper)
    {
        _dbContext = dbContext;
        _logger = logger;
        _mapper = mapper;
    }



    public async Task<List<ClientDto>> GetAllClientsWithChargedSales(BranchOption branch)
    {
        try
        {
            var result = await _dbContext.Clients
            .Where(c => c.Branch == branch)
            .Where(c => c.DailySales.Any(ds => ds.IsChargedSales)) // 🔑 KEY LINE
            .AsNoTracking()
            .Select(c => new ClientDto
            {
                Id = c.Id,
                ClientName = c.ClientName,
                Address = c.Address,
                ContactNumber = c.ContactNumber,
                Branch = c.Branch,

                DailySales = c.DailySales
                    .Where(ds => ds.IsChargedSales)
                    .Select(ds => new DailySaleDto
                    {
                        Id = ds.Id,
                        DateOfSales = ds.DateOfSales,
                        TotalAmount = ds.TotalAmount,
                        IsChargedSales = ds.IsChargedSales
                    })
                    .ToList()
            })
            .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            // LOG THIS (do not swallow silently)
             _logger.LogError(ex, "Failed to get clients with charged sales.");

            throw; // rethrow so you can see the real error
        }
    }

    public async Task<ClientDto?> GetClientChargeSales(int clientId)
    {
        try
        {
            var client = await _dbContext.Clients
                .Where(c => c.Id == clientId)
                .AsNoTracking()
                
                .Select(c => new ClientDto
                {
                    Id = c.Id,
                    ClientName = c.ClientName,
                    Address = c.Address,
                    ContactNumber = c.ContactNumber,
                    Branch = c.Branch,

                    DailySales = c.DailySales
                        .Where(ds => ds.IsChargedSales)
                        .Select(ds => new DailySaleDto
                        {
                            Id = ds.Id,
                            DateOfSales = ds.DateOfSales,
                            TotalAmount = ds.TotalAmount,
                            IsChargedSales = ds.IsChargedSales,
                            RecieptReference = ds.RecieptReference,
                            SalesNumber = ds.SalesNumber,
                            // Only count, no details
                            SaleItemsCount = ds.SaleItems.Count()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get client charge sales for ClientId: {ClientId}", clientId);
            throw;
        }
    }


    public async Task<bool> UpdateChargeSale(int clientId, ClientDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        try
        {
            // Get client from DB
            var client = await _dbContext.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
                return false; // client not found

            // Update remaining balance
            client.RemainingChargeBalance = dto.RemainingChargeBalance ?? 0m;



            // Save changes
            await _dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update remaining charge for ClientId {ClientId}", clientId);
            return false;
        }
    }

    public async Task<byte[]> GenerateStatementPdfAsync(int clientId)
    {
        var client = await _dbContext.Clients
            .AsNoTracking()
            .Where(c => c.Id == clientId)
            .Select(c => new
            {
                c.ClientName,
                c.Branch,
                c.RemainingChargeBalance,
                c.Address,
                DailySales = c.DailySales
                    .Where(ds => ds.IsChargedSales)
                    .Select(ds => new DailySaleDto
                    {
                        DateOfSales = ds.DateOfSales,
                        RecieptReference = ds.RecieptReference,
                        SalesNumber = ds.SalesNumber,
                        TotalAmount = ds.TotalAmount,
                        SaleItemsCount = ds.SaleItems.Count()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (client == null)
            throw new Exception("Client not found.");

        var chargedSalesTotal =
            client.DailySales.Sum(x => x.TotalAmount ?? 0m);

        var remainingBalance =
            client.RemainingChargeBalance ?? 0m;

        var dto = new StatementReportDocumentDto
        {
            ClientName = client.ClientName,
            Branch = client.Branch,
            DailySales = client.DailySales,

            Credit = remainingBalance,
            Balance = chargedSalesTotal + remainingBalance,
            Address = client.Address,
            ReportGenerated = DateTime.Now
        };

        var document = new StatementReportDocument(dto);
        return document.GeneratePdf();
    }

}





public interface IStatementReportService
{
    Task<List<ClientDto>> GetAllClientsWithChargedSales(BranchOption branch);
    Task<ClientDto?> GetClientChargeSales(int clientId);
    Task<byte[]> GenerateStatementPdfAsync(int clientId);
    Task<bool> UpdateChargeSale(int clientId, ClientDto dto);
}
