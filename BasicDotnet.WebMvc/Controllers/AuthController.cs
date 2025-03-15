using BasicDotnet.App.Services;
using BasicDotnet.WebMvc.Configurations;
using BasicDotnet.WebMvc.Models.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BasicDotnet.WebMvc.Controllers;

public class AuthController : BaseController
{
    private readonly string _registerApiEndpoint;
    private readonly string _loginApiEndpoint;

    public AuthController(IOptions<ApiConfig> apiConfigOption) : base(apiConfigOption)
    {
        _registerApiEndpoint = $"{_baseApiUrl}/customer/register";
        _loginApiEndpoint = $"{_baseApiUrl}/customer/login";
    }

    [HttpGet("register")]
    public IActionResult Register()
    {
        var model = new RegisterViewModel
        {
            RegisterApiEndpoint = _registerApiEndpoint
        };

        return View(model);
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var model = new LoginViewModel
        {
            LoginApiEndpoint = _loginApiEndpoint
        };

        return View(model);
    }

    [HttpPost("set-auth-cookie")]
    public IActionResult SetAuthCookie([FromBody] TokenResponse tokenResponse)
    {
        if (string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            return BadRequest("Invalid token");
        }

        // Store JWT token as a secure, HTTP-only cookie
        Response.Cookies.Append("accessToken", tokenResponse.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenResponse.AccessTokenExpires
        });

        // Store Refresh Token (optional)
        Response.Cookies.Append("refreshToken", tokenResponse.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = tokenResponse.RefreshTokenExpires
        });

        return Ok(new { message = "Token stored in cookies" });
    }

}
