using BasicDotnet.App.Attributes;
using BasicDotnet.App.Dtos;
using BasicDotnet.App.Services;
using BasicDotnet.Domain.Enums;
using BasicDotnet.Domain.Exceptions;
using BasicDotnet.Domain.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Controllers;

public class CustomerController : BaseController
{
    private readonly UserRole _userRole = UserRole.Customer;

    public readonly AuthService _authService;

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
        // Get the current user and role from claims
        var currentUserId = GetCurrentUserId();
        var currentRole = GetCurrentRoleId();

        if (currentUserId == null || currentRole == null)
        {
            return Error("Invalid or missing user ID or role.", 400);
        }

        var user = await _authService.GetUserByIdAsync(currentUserId.Value, currentRole.Value);

        return Success(user);
    }

    [HttpGet("{user_id}")]
    [HasPermission(PermissionNames.ViewAllCustomers)]
    [HasPermission(PermissionNames.ViewOwnCustomer)]
    public async Task<IActionResult> GetUserByIdAsync(Guid user_id)
    {
        var currentUserId = GetCurrentUserId();
        var currentRole = GetCurrentRoleId();

        if (currentRole == UserRole.Customer && currentUserId != user_id)
        {
            return Error("Access denied: You are not authorized to view details of other users with this role.", 403);
        }

        var user = await _authService.GetUserByIdAsync(user_id, _userRole);
        return Success(user);
    }
}
