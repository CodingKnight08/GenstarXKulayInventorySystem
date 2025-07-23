using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;


    public AuthenticationController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegistrationDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return BadRequest("Passwords do not match.");

        var user = new User
        {
            UserName = dto.Username,
            Email = dto.Email,
            Role = dto.Role
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null)
            return Unauthorized("Invalid username or password.");

        var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!valid)
            return Unauthorized("Invalid username or password.");

        // Return JWT or user info
        return Ok(new { user.UserName, user.Email, user.Role });
    }

}
