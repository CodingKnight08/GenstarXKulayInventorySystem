using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using static GenstarXKulayInventorySystem.Shared.Helpers.UtilitiesHelper;

namespace GenstarXKulayInventorySystem.Server.Services;

public class WayBillService: IWayBillService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<WayBillService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WayBillService(
        InventoryDbContext context,
        IMapper mapper,
        ILogger<WayBillService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }


    public async Task<List<WayBillDto>> GetAllWayBills()
    {
        var wayBills = await _context.WayBills
            .AsNoTracking()
            .AsSplitQuery()
            .Include(wb => wb.Supplier)
            .Include(wb => wb.WayBillItems)
            .ThenInclude(wi => wi.BranchProduct) // optional if you need product info
            .ToListAsync();

        if (wayBills == null || wayBills.Count == 0)
        {
            _logger.LogInformation("No waybills found in the database.");
            return new List<WayBillDto>();
        }

        var wayBillDtos = _mapper.Map<List<WayBillDto>>(wayBills);

        foreach (var dto in wayBillDtos)
        {
            dto.TotalAmount = dto.WayBillItems.Sum(i => i.TotalPrice);
        }

        return wayBillDtos;
    }


    public async Task<WayBillDto?> GetWayBillById(int wayBill)
    {
        WayBill? wayBillEntity = await _context.WayBills
                            .AsNoTracking()
                            .AsSplitQuery()
                            .Include(wb => wb.Supplier)
                            .FirstOrDefaultAsync(wb => wb.Id == wayBill);
        if (wayBillEntity == null)
        {
            _logger.LogWarning("Waybill with ID {WayBillId} not found.", wayBill);
            return null;
        }
        WayBillDto wayBillDto = _mapper.Map<WayBillDto>(wayBillEntity);
        return wayBillDto;
    }

    public async Task<List<WayBillDto>> GetWayBillsBySupplierId(int supplierId)
    {
        List<WayBill> wayBills = await _context.WayBills
                            .AsNoTracking()
                            .AsSplitQuery()
                            .Include(wb => wb.Supplier)
                            .Include(wb => wb.WayBillItems)
                                .ThenInclude(wbi => wbi.BranchProduct)
                            .Where(wb => wb.SupplierId == supplierId)
                            .ToListAsync();
        if (wayBills == null || wayBills.Count == 0)
        {
            _logger.LogInformation("No waybills found for Supplier ID {SupplierId}.", supplierId);
            return new List<WayBillDto>();
        }
        List<WayBillDto> wayBillDtos = _mapper.Map<List<WayBillDto>>(wayBills);
        return wayBillDtos;
    }

    public async Task<List<WayBillItemsDto>> GetAllWayBillItemsByWayBillId(int waybillId)
    {
        List<WayBillItems> waybillItems = await _context.WayBillItems
                                            .AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(wb => wb.BranchProduct)
                                            .ThenInclude(wb => wb.MasterProduct)
                                            .Where(wb => wb.WayBillId == waybillId)
                                            .ToListAsync();
        if (waybillItems.Count == 0 || waybillItems is null)
        {
            _logger.LogInformation("No waybill items found {WaybillId}", waybillId);
            return new List<WayBillItemsDto>();
        }
        List<WayBillItemsDto> wayBillItems = _mapper.Map<List<WayBillItemsDto>>(waybillItems);
        return wayBillItems;
    }

    public async Task<bool> CreateWayBill(WayBillDto wayBillDto)
    {
        try
        { 

            var exist = await _context.WayBills
                            .AsNoTracking()
                            .FirstOrDefaultAsync(wb => wb.WayBillNumber == wayBillDto.WayBillNumber);
            if (exist != null)
            {
                return false;
            }
            WayBill wayBillEntity = _mapper.Map<WayBill>(wayBillDto);
            wayBillEntity.DateReceived = PhilippineTime.Now; 
            wayBillEntity.CreatedBy = GetCurrentUsername();
            wayBillEntity.CreatedAt = PhilippineTime.Now;
            await _context.WayBills.AddAsync(wayBillEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating waybill.");
            return false;
        }
    }

    public async Task<bool> DeleteWayBill(int wayBillId)
    {
        try
        {
            var wayBillEntity = await _context.WayBills
                                    .Include(wb => wb.WayBillItems)
                                    .FirstOrDefaultAsync(wb => wb.Id == wayBillId);
            if (wayBillEntity == null)
            {
                _logger.LogWarning("Waybill with ID {WayBillId} not found for deletion.", wayBillId);
                return false;
            }
            _context.WayBills.Remove(wayBillEntity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting waybill with ID {WayBillId}.", wayBillId);
            return false;
        }
    }
}
public interface IWayBillService
{
    Task<List<WayBillDto>> GetAllWayBills();
    Task<WayBillDto?> GetWayBillById(int wayBill);
    Task<List<WayBillDto>> GetWayBillsBySupplierId(int supplierId);
    Task<List<WayBillItemsDto>> GetAllWayBillItemsByWayBillId(int waybillId);
    Task<bool> CreateWayBill(WayBillDto wayBillDto);
    Task<bool> DeleteWayBill(int wayBillId);
}