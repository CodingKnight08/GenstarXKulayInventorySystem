using AutoMapper;
using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using GenstarXKulayInventorySystem.Shared.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace GenstarXKulayInventorySystem.Server.Services;

public class AuthenticationService: IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly InventoryDbContext _context;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IMapper _mapper;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager, InventoryDbContext context, IMapper mapper, ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        return _mapper.Map<List<UserDto>>(users);
    }
    public async Task<bool> RegisterAsync(RegistrationDto registerDto)
    {
        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            Role = UserRole.User,
            Branch = registerDto.Branch,
            PhoneNumber = registerDto.ContactNumber,
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        return result.Succeeded;
    }

    public async Task<bool> RegisterUser(RegistrationDto registration)
    {
        try
        {
            var existingRegistrant = await _context.Registrations
                .FirstOrDefaultAsync(e => e.Email == registration.Email && e.FullName == registration.FullName);

            if (existingRegistrant != null) {
                return false;
            }

            var registrant = _mapper.Map<Registration>(registration);
            registrant.CreatedAt = UtilitiesHelper.GetPhilippineTime();
            await _context.Registrations.AddAsync(registrant);
            int result = await _context.SaveChangesAsync();
            return result > 0;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Email}", registration.Email);
            return false;
        }
    }

    public async Task<bool> LoginAsync(LoginDto loginDto)
    {
        var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false);
        return result.Succeeded;
    }


    public async Task<List<RegistrationDto>> GetAllRegistrationsAsync()
    {
        List<Registration> registrants = await _context.Registrations.AsNoTracking().AsSplitQuery()
            .Where(r => !r.IsDeleted && !r.IsApproved).ToListAsync();

        if(registrants == null || !registrants.Any())
        {
            return new List<RegistrationDto>();
        }
        List<RegistrationDto> registrantDtos = _mapper.Map<List<RegistrationDto>>(registrants);
        return registrantDtos;
    }

    public async Task<RegistrationDto> GetRegistrant(int id)
    {
        var registrant = await _context.Registrations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && !r.IsApproved);
        if (registrant == null)
        {
            return new RegistrationDto();
        }
        var registrantDto = _mapper.Map<RegistrationDto>(registrant);
        return registrantDto;
    }

    public async Task<bool> UpdateRegistrant(RegistrationDto registrationDto)
    {
        try
        {
            var existingRegistrant = await _context.Registrations.FirstOrDefaultAsync(r => r.Id == registrationDto.Id && !r.IsDeleted && !r.IsApproved);
            if (existingRegistrant == null)
            {
                return false;
            }
            existingRegistrant.FullName = registrationDto.FullName;
            existingRegistrant.Email = registrationDto.Email;
            existingRegistrant.ContactNumber = registrationDto.ContactNumber;
            existingRegistrant.Branch = registrationDto.Branch;
            existingRegistrant.Password = registrationDto.Password;
            existingRegistrant.ConfirmPassword = registrationDto.ConfirmPassword;
            existingRegistrant.IsApproved = registrationDto.IsApproved;
            existingRegistrant.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
            _context.Registrations.Update(existingRegistrant);
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating registrant: {Id}", registrationDto.Id);
            return false;
        }
    }

    public async Task<bool> UpdateUser(UserDto userDto)
    {
        try
        {
            var existingUser = await _userManager.FindByIdAsync(userDto.Id.ToString());
            if (existingUser == null)
            {
                return false;
            }
            existingUser.UserName = userDto.Username;
            existingUser.Email = userDto.Email;
            existingUser.Role = userDto.Role;
            existingUser.Branch = userDto.Branch;
            var result = await _userManager.UpdateAsync(existingUser);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Id}", userDto.Id);
            return false;
        }
    }

    public async Task<bool> ApproveApplicant(int id)
    {
               try
        {
            var registrant = await _context.Registrations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted && !r.IsApproved);
            if (registrant == null)
            {
                return false;
            }
            var user = new User
            {
                UserName = registrant.FullName,
                Email = registrant.Email,
                Role = UserRole.User,
                Branch = registrant.Branch,
                PhoneNumber = registrant.ContactNumber,
            };
            var result = await _userManager.CreateAsync(user, registrant.Password);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create user for registrant: {Id}", id);
                return false;
            }
            registrant.IsApproved = true;
            registrant.UpdatedAt = UtilitiesHelper.GetPhilippineTime();
            _context.Registrations.Update(registrant);
            int saveResult = await _context.SaveChangesAsync();
            return saveResult > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving applicant: {Id}", id);
            return false;
        }
    }
}

public interface IAuthenticationService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<bool> RegisterUser(RegistrationDto registration);
    Task<bool> RegisterAsync(RegistrationDto registerDto);
    Task<bool> LoginAsync(LoginDto loginDto);
    Task<bool> UpdateUser(UserDto userDto);

    Task<List<RegistrationDto>> GetAllRegistrationsAsync();
    Task<RegistrationDto> GetRegistrant(int id);
    Task<bool> UpdateRegistrant(RegistrationDto registrationDto);
    Task<bool> ApproveApplicant(int id);
}