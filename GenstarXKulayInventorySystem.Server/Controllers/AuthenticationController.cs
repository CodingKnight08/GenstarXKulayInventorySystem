using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GenstarXKulayInventorySystem.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthenticationController> _logger;


    public AuthenticationController(IAuthenticationService authService, UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtService jwtService,
        ILogger<AuthenticationController> logger
        )
    {
        _authService = authService;
        _userManager = userManager;
        _jwtService = jwtService;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet("all/applicants")]
    public async Task<IActionResult> GetAllApplicants()
    {
        var applicants = await _authService.GetAllRegistrationsAsync();
        return Ok(applicants);
    }

    [HttpGet("all/users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("applicant/{id}")]
    public async Task<IActionResult> GetApplicantById(int id)
    {
        var applicant = await _authService.GetRegistrant(id);
        return applicant != null ? Ok(applicant) : NotFound("Applicant not found.");
    }

    [HttpPut("approve-applicant/{id}")]
    public async Task<IActionResult> ApproveApplicant(int id)
    {
        var result = await _authService.ApproveApplicant(id);
        return result ? Ok(result) : BadRequest("Approval failed or applicant not found.");
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, UserDto updateUserDto)
    {
        if (id != updateUserDto.Id.ToString())
            return BadRequest("ID mismatch.");

        var result = await _authService.UpdateUser(updateUserDto);
        return result ? Ok(result) : BadRequest("Update failed or user not found.");
    }


    [HttpPost("user-registration")]
    public async Task<IActionResult> Register(RegistrationDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        return result ? Ok(result) : BadRequest("Registration failed.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(RegistrationDto registrationDto)
    {
        var result = await _authService.RegisterUser(registrationDto);
        return result ? Ok(result) : BadRequest("User registration failed or user already exists.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var token = await _authService.LoginAsync(loginDto);
        if (token == null)
            return Unauthorized();

        return Ok(new { Token = token });
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromBody] UserDto userDto)
    {
        if (userDto == null || string.IsNullOrWhiteSpace(userDto.Username))
            return BadRequest("Invalid user data.");

        try
        {
            var result = await _authService.DeleteUser(userDto);

            if (!result)
                return NotFound($"User '{userDto.Username}' not found or already deleted.");

            return Ok(new { message = $"User '{userDto.Username}' was soft deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Username}", userDto.Username);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the user.");
        }
    }
}
