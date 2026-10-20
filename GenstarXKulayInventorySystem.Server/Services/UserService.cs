using AutoMapper;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static GenstarXKulayInventorySystem.Shared.Helpers.ProductsEnumHelpers;

namespace GenstarXKulayInventorySystem.Server.Services;

 public class UserService:IUserService
{
    private readonly InventoryDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserService> _logger;

    public UserService(InventoryDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    private string GetCurrentUsername()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null) return "Unknown";

        var usernameClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }
     
    public async Task<List<UserDto>> GetStaffsAsync(BranchOption branch)
    {
        var staffs = await _context.Users
            .Where(u => (u.Role == UserRole.Staff || u.Role == UserRole.Secretary) && u.Branch == branch)
            .ToListAsync();
        return _mapper.Map<List<UserDto>>(staffs);
    }

    public async Task<bool> AssignBranchProductStaff(BranchProductDto dto)
    {
        try
        {
            var branchProduct = await _context.BranchProducts.FindAsync(dto.Id);
            if (branchProduct == null)
            {
                _logger.LogWarning($"Branch product with ID {dto.Id} not found.");
                return false;
            }
            branchProduct.UserId = dto.UserId;
            branchProduct.UpdatedBy = GetCurrentUsername();
            branchProduct.UpdatedAt = DateTime.UtcNow;
            _context.BranchProducts.Update(branchProduct);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while assigning staff to the branch product.");
            return false;
        }
    }
}


public interface IUserService
{
    Task<List<UserDto>> GetStaffsAsync(BranchOption branch);
    Task<bool> AssignBranchProductStaff(BranchProductDto dto);
}