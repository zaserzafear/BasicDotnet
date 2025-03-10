using BasicDotnet.App.Dtos;
using BasicDotnet.App.Services;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BasicDotnet.WebApi.Controllers;

public class CustomerController : BaseController
{
    private readonly AuthService _authService;
    private readonly Domain.Enums.UserRole _userRole = Domain.Enums.UserRole.Customer;

    public CustomerController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var user = await _authService.AddUserAsync(dto.UserName, dto.Email, dto.Password, _userRole);
            return Success(new { userId = user.Id }, "User registered successfully");
        }
        catch (UserAlreadyExistsException ex)
        {
            return Error(ex.Message, 409);
        }
        catch (Exception ex)
        {
            return Error($"An unexpected error occurred. {ex.Message}", 500);
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var token = await _authService.AuthenticateAsync(dto.UserName, dto.Password, _userRole);
            if (token == null)
                return Error("Invalid username or password", 401);

            return Success(new { Token = token }, "Login successful");
        }
        catch (Exception ex)
        {
            return Error($"An unexpected error occurred. {ex.Message}", 500);
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync()
    {
        // Retrieve the GUID user ID from the Name claim
        var userIdClaim = HttpContext.User.Identity?.Name;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return BadRequest("Invalid or missing user ID.");
        }

        // Retrieve the role claim and map it to the UserRole enum
        var roleClaim = HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (!Enum.TryParse(roleClaim, out UserRole role))
        {
            return BadRequest("Invalid or missing role.");
        }

        var user = await _authService.GetUserByIdAsync(userId, role);

        return Success(user);
    }
}
