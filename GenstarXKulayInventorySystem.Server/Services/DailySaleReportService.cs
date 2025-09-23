using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using static GenstarXKulayInventorySystem.Shared.Helpers.OrdersHelper;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Services;

public class DailySaleReportService : IDailySaleReportService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<DailySaleReportService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DailySaleReportService(InventoryDbContext context, IMapper mapper, ILogger<DailySaleReportService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<DailySaleReportDto>> GetAllDailyReportAsync(BranchOption branch)
    {
        List<DailySaleReport> reports = await _context.DailySaleReports
            .AsNoTracking()
            .AsSplitQuery()
            .Where(dr => !dr.IsDeleted && dr.Branch == branch)
            .OrderBy(dr => dr.Date)
            .ToListAsync();

        if (reports == null || !reports.Any())
        {
            return new List<DailySaleReportDto>();
        }

        List<DailySaleReportDto> reportDtos = _mapper.Map<List<DailySaleReportDto>>(reports);
        return reportDtos;
    }

    public async Task<DailySaleReportDto?> GetDailyReportByIdAsync(int id)
    {
        DailySaleReport? report = await _context.DailySaleReports
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(dr => dr.Id == id && !dr.IsDeleted);
        if (report == null)
        {
            return null;
        }
        DailySaleReportDto reportDto = _mapper.Map<DailySaleReportDto>(report);
        return reportDto;

    }

   

    public async Task<bool> AddReportAsync(DailySaleReportDto reportDto)
    {
        try
        {
            var existingReport = await _context.DailySaleReports.AsNoTracking().FirstOrDefaultAsync(dr => !dr.IsDeleted && dr.Date.Date == DateTime.UtcNow.Date && dr.Branch == reportDto.Branch);
            if(existingReport != null)
            {
                return false; 
            }
            var report = _mapper.Map<DailySaleReport>(reportDto);
            report.Date = DateTime.UtcNow;
            report.CreatedAt = DateTime.UtcNow;
            report.CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";

            _ = await _context.DailySaleReports.AddAsync(report);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding daily sale report");
            return false;
        }
    }


    public async Task<bool> UpdateReportAsync(DailySaleReportDto reportDto)
    {
        try
        {
            var existingReport = await _context.DailySaleReports.FirstOrDefaultAsync(dr => dr.Id == reportDto.Id && !dr.IsDeleted);
            if (existingReport == null)
            {
                return false; // Report not found
            }
            // Map updated fields from DTO to entity
            _mapper.Map(reportDto, existingReport);
            existingReport.UpdatedAt = DateTime.UtcNow;
            existingReport.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            _context.DailySaleReports.Update(existingReport);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating daily sale report");
            return false;
        }
    }

    public async Task<bool> DeleteReportAsync(int id)
    {
        try
        {
            var existingReport = await _context.DailySaleReports.FirstOrDefaultAsync(dr => dr.Id == id && !dr.IsDeleted);
            if (existingReport == null)
            {
                return false; // Report not found
            }
            existingReport.IsDeleted = true;
            existingReport.DeletedAt = DateTime.UtcNow;
            _context.DailySaleReports.Update(existingReport);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting daily sale report");
            return false;
        }
    }

    public async Task<List<DailySaleDto>> GetAllDailySaleInvoice(DateTime date, BranchOption branch)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var paidDailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.Branch == branch
                      && ds.SalesOption == PurchaseRecieptOption.BIR
                      && ds.UpdatedAt == null
                      && ds.PaymentType != null
                      && ds.DateOfSales >= start
                      && ds.DateOfSales < end)
            .ToListAsync();

        if (paidDailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(paidDailySales);
    }
    public async Task<List<DailySaleDto>> GetAllDailySaleNonInvoice(DateTime date, BranchOption branch)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var paidDailySales = await _context.DailySales
            .AsNoTracking()
            .AsSplitQuery()
            .Where(ds => !ds.IsDeleted
                      && ds.Branch == branch
                      && ds.SalesOption == PurchaseRecieptOption.NonBIR
                      && ds.UpdatedAt == null
                      && ds.PaymentType != null
                      && ds.DateOfSales >= start
                      && ds.DateOfSales < end)
            .ToListAsync();

        if (paidDailySales.Count == 0)
            return new List<DailySaleDto>();

        return _mapper.Map<List<DailySaleDto>>(paidDailySales);
    }
}

public interface IDailySaleReportService
{
    Task<List<DailySaleReportDto>> GetAllDailyReportAsync(BranchOption branch);
    Task<DailySaleReportDto?> GetDailyReportByIdAsync(int id);
    Task<bool> AddReportAsync(DailySaleReportDto reportDto);
    Task<bool> UpdateReportAsync(DailySaleReportDto reportDto);
    Task<bool> DeleteReportAsync(int id);


    Task<List<DailySaleDto>> GetAllDailySaleInvoice(DateTime date, BranchOption branch);
    Task<List<DailySaleDto>> GetAllDailySaleNonInvoice(DateTime date, BranchOption branch);
}
