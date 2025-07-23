using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Identity;

namespace GenstarXKulayInventorySystem.Server.Services;

public class AuthenticationService: IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> RegisterAsync(RegistrationDto registerDto)
    {
        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            Role = UserRole.User
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        return result.Succeeded;
    }

    public async Task<bool> LoginAsync(LoginDto loginDto)
    {
        var result = await _signInManager.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false);
        if (result.Succeeded)
        {
            return false; // Replace with token if JWT
        }
        return true;
    }
}

public interface IAuthenticationService
{
    Task<bool> RegisterAsync(RegistrationDto registerDto);
    Task<bool> LoginAsync(LoginDto loginDto);
}