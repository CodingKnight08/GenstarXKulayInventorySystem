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
                        .AsNoTracking()
                        .Where(c => c.Branch == branch)
                        .Where(c => c.DailySales.Any(ds => ds.IsChargedSales))
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
                        .Where(ds => ds.IsChargedSales && !ds.IsPaid)
                        .Select(ds => new DailySaleDto
                        {
                            Id = ds.Id,
                            DateOfSales = ds.DateOfSales,
                            TotalAmount = ds.TotalAmount,
                            IsChargedSales = ds.IsChargedSales,
                            RecieptReference = ds.RecieptReference,
                            SalesNumber = ds.SalesNumber,
                            
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
    public async Task<StatementOfAccountDataDto?> GetChargeSalesForThatMonth(
    int clientId,
    int month,
    int year)
    {
        try
        {
            var client = await _dbContext.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clientId);

            if (client is null)
                return null;

            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var chargedSales = await _dbContext.DailySales
                .AsNoTracking()
                .Include(ds => ds.SaleItems)
                .Where(c =>
                    c.ClientId == clientId &&
                    c.IsChargedSales &&
                    !c.IsPaid)
                .ToListAsync();

            List<DailySaleDto> mappedChargeSales =
                _mapper.Map<List<DailySaleDto>>(chargedSales);

            var previousUnpaidSalesTotal =
                mappedChargeSales
                    .Where(s => s.DateOfSales < monthStart)
                    .Sum(s => s.TotalAmount ?? 0m);

            var currentMonthSales =
                mappedChargeSales
                    .Where(s =>
                        s.DateOfSales >= monthStart &&
                        s.DateOfSales < monthEnd)
                    .ToList();

            var beginningBalance =
                (client.RemainingChargeBalance ?? 0m)
                + previousUnpaidSalesTotal;

            return new StatementOfAccountDataDto
            {
                ClientName = client.ClientName,
                BeginningBalance = beginningBalance,
                ChargeSales = currentMonthSales,
                Branch = client.Branch,
                
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return null;
        }
    }


    public Task<byte[]> GenerateStatementPdfAsync(StatementOfAccountDataDto soaDto)
    {
        if (soaDto is null)
            throw new ArgumentNullException(nameof(soaDto));

        try
        {
            var document = new StatementReportDocument(soaDto);
            var pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }
        catch (Exception ex)
        {
            // Log the exception if you have a logger
            // _logger.LogError(ex, "Error generating PDF for client {ClientName}", soaDto.ClientName);

            // Optionally, rethrow or wrap in a custom exception
            throw new InvalidOperationException(
                $"Failed to generate PDF for client '{soaDto.ClientName}'.", ex);
        }
    }



}





public interface IStatementReportService
{
    Task<List<ClientDto>> GetAllClientsWithChargedSales(BranchOption branch);
    Task<ClientDto?> GetClientChargeSales(int clientId);
    Task<byte[]> GenerateStatementPdfAsync(StatementOfAccountDataDto soaDto);
    Task<bool> UpdateChargeSale(int clientId, ClientDto dto);
    Task<StatementOfAccountDataDto?> GetChargeSalesForThatMonth(int clientId, int month,int year);
}
