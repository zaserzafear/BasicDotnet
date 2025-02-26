using BasicDotnet.App.Dtos;
using BasicDotnet.App.Services;
using Microsoft.AspNetCore.Mvc;

namespace BasicDotnet.WebApi.Controllers;

public class AuthController : BaseController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        if (await _authService.RegisterAsync(model.UserName, model.Email, model.Password))
            return Ok(new { Message = "User registered successfully" });

        return BadRequest("User already exists.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var token = await _authService.LoginAsync(model.UserName, model.Password);
        if (token == null)
            return Unauthorized();

        return Ok(new { Token = token });
    }
}
