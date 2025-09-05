using GenstarXKulayInventorySystem.Server.Model;
using GenstarXKulayInventorySystem.Server.Services;
using GenstarXKulayInventorySystem.Shared.DTOS;
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


    public AuthenticationController(IAuthenticationService authService, UserManager<User> userManager,
        SignInManager<User> signInManager,
        JwtService jwtService)
    {
        _authService = authService;
        _userManager = userManager;
        _jwtService = jwtService;
        _signInManager = signInManager;
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
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto, [FromServices] JwtService jwtService)
    {
        var user = await _userManager.FindByNameAsync(loginDto.Username);
        if (user == null) return Unauthorized("Invalid username or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded) return Unauthorized("Invalid username or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = jwtService.GenerateToken(user, roles);

        return Ok(new { Token = token });
    }


}
