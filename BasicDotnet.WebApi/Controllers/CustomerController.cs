using BasicDotnet.App.Dtos;
using BasicDotnet.App.Services;
using BasicDotnet.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Controllers;

public class CustomerController : BaseController
{
    private readonly AuthService _authService;
    private readonly Domain.Enums.UserRole _userRole = Domain.Enums.UserRole.Customer;

    public CustomerController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        string requestId = HttpContext.TraceIdentifier;

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
}
